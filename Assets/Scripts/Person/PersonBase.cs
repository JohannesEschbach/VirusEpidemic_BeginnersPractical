using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public abstract class PersonBase : MonoBehaviour
{
    #region General
    public int ID;
    public bool Alive = true;
    public static int nextID;
    private Data data = Data.Instance;
    private NetworkGraph networkGraph = NetworkGraph.Instance;
    public UpdateMode updateMode;
    private RoutineMgr routineMgr;
    public InfectionMgr infectionMgr { get; private set; }

    public void OnDeath()
    {
        if(updateMode == UpdateMode.INSIDE) LeaveBuilding();
        LeaveModule();
        data.LeaveMeeting(this);
        transform.position = new Vector3(0, -200, 0); //Just to go sure i guess
        Alive = false;
        Stats.currentInfected--;
        if (infectionMgr.confirmedInfected)
        {
            Stats.confirmedCurrentInfected--;
            infectionMgr.confirmedInfected = false;
        }
        else
        {
            Stats.confirmedTotalInfected++;
        }
        if (data.personIsHospitalisedIn(this) != null)
        {
            data.personIsHospitalisedIn(this).LeaveHospital(this);            
        }
        Stats.fatalities++;
        gameObject.SetActive(false);
    }

    public UpdateMode GetUpdateMode()
    {
        return updateMode;
    }

    public void SetUpdateMode(UpdateMode mode)
    {
        if (updateMode != UpdateMode.MODULEOUTSIDE && mode == UpdateMode.MODULEOUTSIDE)
        {
            if(Alive) gameObject.SetActive(true);
            animator.Play("Walk");
            transform.position = lastVisitedVertex.transform.position;
        }
        if (updateMode != UpdateMode.CITYOUTSIDE && mode == UpdateMode.CITYOUTSIDE)
        {
            wokeUp = false;
        }
        updateMode = mode;
    }

    public void SetUpdateModeOnFocusSwitch(UpdateMode mode, Module module) 
    {
        
        if(updateMode == UpdateMode.CITYOUTSIDE && mode == UpdateMode.MODULEOUTSIDE)
        {
            //ON MAIN THREAD!!!
            infectionMgr.LeaveModuleExposureCheckMainThread(module);

            if(travelRoute != null)
            {
                //Find out where on the route the person needs to be placed approximately
                DayTime timeNow = new DayTime();
                float distanceCovered;
                if (wakeUpTime != null)
                {
                    float remainingTravelMin = wakeUpTime - timeNow;
                    distanceCovered = 1 - (remainingTravelMin / inGameMinLeft);

                }
                else distanceCovered = 0;


                int verticesLeftInModule = 0;
                while (verticesLeftInModule < travelRoute.Count() && travelRoute.ElementAt(verticesLeftInModule).moduleID == data.IsInModule(this).ID) { verticesLeftInModule++; }

                int verticesCovered = Mathf.RoundToInt(distanceCovered * verticesLeftInModule);

                for (int i = 0; travelRoute.Count() > 1 && i < verticesCovered; i++) { lastVisitedVertex = travelRoute.Pop(); }
                transform.position = lastVisitedVertex.transform.position;
                if (travelRoute.Count() > 0) transform.LookAt(travelRoute.Peek().transform.position);
            }
        }
        SetUpdateMode(mode);      
    }
    #endregion

    #region Travel
    public float speed = 2f; //DEFAULT between 1 and 4
    private float speedFactorCityUpdateMode = 1.5f; //Allows faster movement on non focused modules. This makes the time progression more adequate without destroying immersion
    public Vertex lastVisitedVertex;
    private bool startedTraversing = false;

    private List<Vertex> routeHomeOffice = null;

    public Stack<Vertex> travelRoute = null;
    public List<Vertex> displayableRouteForDebugging = null;

    public BuildingBase targetBuilding = null;

    public void EnterBuilding(BuildingBase building)
    {
        data.AddPersonToBuilding(this, building);

        infectionMgr.LeaveModuleExposureCheck();
        data.RemovePersonFromModuleOutside(this);

        targetBuilding = null; //Go sure target Building is reset        
        SetUpdateMode(UpdateMode.INSIDE);
        building.OnBuildingEnter(this);
        stayInsideBuildingUntil();
    }

    public void LeaveBuilding()
    {
        data.IsInBuilding(this).OnBuildingExit(this);
        
        infectionMgr.LeaveBuildingExposureCheck();
        data.RemovePersonFromBuilding(this);
        data.AddPersonToModuleOutside(this, data.IsInModule(this));

        if (!data.IsInModule(this).IsFocused())
        {
            SetUpdateMode(UpdateMode.CITYOUTSIDE);
        }
        else
        {
            SetUpdateMode(UpdateMode.MODULEOUTSIDE);
        }
    }

    //Plot a route through the network to a specific building
    void PlotTo(BuildingBase building)
    {
        targetBuilding = building;
        Vertex endVertex = networkGraph.GetBuildingVertex(building.ID); //Get the vertex of the building
        PlotTo(endVertex);
    }

    public Vertex targetVertex; //Bugfixing
    public int waitingForRoute = 0;
    public int routeLength = 0;

    bool PlotTo(Vertex endVertex)
    {
        prevTravelRoute = null;
        travelRoute = null; //reset
        targetVertex = null;

        if (updateMode == UpdateMode.INSIDE)
        {
            lastVisitedVertex = networkGraph.GetBuildingVertex(data.IsInBuilding(this).ID);
        }

        transform.position = lastVisitedVertex.transform.position; //Place Person at Startvertex!

        if (endVertex == networkGraph.GetBuildingVertex(data.GetWorkplace(this).ID) && data.GetHome(this) == data.IsInBuilding(this))
        {
            travelRoute = new Stack<Vertex>(routeHomeOffice.AsEnumerable().Reverse().ToList());
        }
        else if (endVertex == networkGraph.GetBuildingVertex(data.GetHome(this).ID) && data.GetWorkplace(this) == data.IsInBuilding(this))
        {
            travelRoute = new Stack<Vertex>(routeHomeOffice);
        }
        //Plot Route
        else
        {
            waitingForRoute += 1;
            targetVertex = endVertex;
            networkGraph.queueRoute(this, lastVisitedVertex, endVertex);
            //travelRoute = new Stack<Vertex>(networkGraph.ShortestPath(lastVisitedVertex, endVertex));
        }

        //travelRoute.Pop(); //Remove Start Vertex (shortestpath returns a list including the start and end vertex)
        startedTraversing = false;
        wokeUp = false;
        return true;
    }

    #region Travel-CityView

    private float inGameMinLeft; //Time until Module border or destination is reached
    private DayTime wakeUpTime; //Time now + inGameMinLeft 
    public bool wokeUp = false;

    public Stack<Vertex> prevTravelRoute = null;
    public List<Vertex> prevTravelRouteForDebugging = null;

    public bool hadATravelRouteInSocial = false;

    void UpdateTravelStatus()
    {       

        if (travelRoute == null || travelRoute.Count == 0)
        {
            if (prevTravelRoute != null)
            {
                //print("lost route : " + "UpdateMode: " + updateMode + "Routine: " + routineMgr.currentRoutine);
                
                //WHY IS THIS NECESSARY?!? It doesnt work without it and that doesnt make any sense 
                travelRoute = new Stack<Vertex>(prevTravelRoute);
                
                //maybe Try to to count how much time missed and jump ahead in route a bit
            }
            return; //Waiting to receive route
        }
        
        else if(routineMgr.currentRoutine == Routine.SOCIAL)
        {
            hadATravelRouteInSocial = true;
            
        }

        //ENDOFBUGFIXING

        if (!startedTraversing)
        {
            if (travelRoute.Count > 1) travelRoute.Pop(); //Remove Start Vertex (shortestpath returns a list including the start and end vertex) //Avoids unnecessary deactivation
            startedTraversing = true;
        }

        if (wokeUp) //Already Travelling
        {
            wokeUp = false;

            nextVertex = travelRoute.Peek();//lastVisitedVertex;
            while (nextVertex.moduleID == data.IsInModule(this).ID) //Find first vertex on next module (Thats where the person is by now)
            {
                if (travelRoute.Count > 0)
                {
                    nextVertex = travelRoute.Pop();
                }
                else
                {
                    break;
                }
            }
            lastVisitedVertex = nextVertex;
            transform.position = lastVisitedVertex.transform.position;

            if (travelRoute.Count > 0)//reachedFinalDestination)
            {
                LeaveModule();
                EnterModule(data.GetModuleFromID(nextVertex.moduleID));
            }
            else
            {
                startedTraversing = false; //reset for next time

                if (targetBuilding != null)
                {
                    EnterBuilding(targetBuilding);
                }
                return;
            }
        }
        
        inGameMinLeft = InGameMinToReachNextStop();
        DayTime timeOnModuleEnter = new DayTime();

        timeOnModuleEnter.AddUnscaledMinute(inGameMinLeft);
        wakeUpTime = new DayTime(timeOnModuleEnter);

        SleepHandlerQueue.SetWakeUpTime(wakeUpTime, this);
        wokeUp = true;
        gameObject.SetActive(false);
    }

    float InGameMinToReachNextStop()
    {
        Module currentModule = data.IsInModule(this);
        
        List<Vertex> verticesRemainingInModule = new List<Vertex>();
        foreach(Vertex vertex in travelRoute)
        {
            if (vertex.moduleID == currentModule.ID) verticesRemainingInModule.Add(vertex);
            else break;
        }

        //Calculate Distance yet to Travel in this Module/Until Target Building:
        float distance = 0;
        Vector3 previousStopPosition = transform.position;
        foreach(Vertex vertex in verticesRemainingInModule)
        {
            distance += Vector3.Distance(vertex.transform.position, previousStopPosition);
            previousStopPosition = vertex.transform.position;
        }

        float inGameMinToWalk = distance / (speed * speedFactorCityUpdateMode);    //InGame Minutes (real time seconds at normal game speed) to walk:
        return inGameMinToWalk;
    }

    void EnterModule(Module module) //REWRITE
    {
        data.AddPersonToModule(this, module, true);

        if (module.IsFocused())
        {
            SetUpdateMode(UpdateMode.MODULEOUTSIDE);
        }
        else 
        {
            SetUpdateMode(UpdateMode.CITYOUTSIDE);
        }
    }

    void LeaveModule()
    {
        infectionMgr.LeaveModuleExposureCheck();
        data.RemovePersonFromModule(this);
    }

    #endregion

    #region Travel-ModuleView: Movement code

    public List<PersonBase> nearbyPersons = new List<PersonBase>();
    private PersonBase nearestPerson;    
    public float currentSpeed;
    private float turnSpeed; //turning speed of person
    private Animator animator;

    ///<summary>Method <c>UpdateNearbyPersons</c> checks for nearby persons and updates nearbyPersons as well as nearestPerson and InfectionMgr.nearbyInfected</summary>
    private void UpdateNearbyPersons()
    {
        nearbyPersons.Clear();
        infectionMgr.nearbyInfected.Clear();
        float nearestPersonDistance = 100;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 7, 1 << 8);

        foreach (Collider other in hitColliders)
        {
            if (other.gameObject != this.gameObject)
            {
                if (other.transform.position == transform.position) transform.position += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)); //if both Persons have same exact position, they will be stuck. Move them by some randomized vector.                
                PersonBase otherPerson = other.gameObject.GetComponent<PersonBase>();
                nearbyPersons.Add(otherPerson);
                float distanceToOther = Vector3.Distance(other.transform.position, transform.position);
                if (distanceToOther < nearestPersonDistance)
                {
                    nearestPersonDistance = distanceToOther;
                    nearestPerson = otherPerson;
                }


                if (!infectionMgr.infected)
                {
                    if (otherPerson.infectionMgr.infected && Vector3.Distance(otherPerson.transform.position, transform.position) <= 5) //Check if infected persons are closer than 5m
                    {
                        lock (InfectionMgr.threadLocker2)
                        {
                            if (!infectionMgr.exposedTo.ContainsKey(otherPerson)) infectionMgr.exposedTo.Add(otherPerson, 0); //Add to exposedTo if not there
                            infectionMgr.nearbyInfected.Add(otherPerson); //Add to nearbyInfected to later compare with exposedTo
                        }

                    }
                }
            }
        }
    }

    public Vertex nextVertex; //DEBUGGING

    private void TraverseRoute() //call in fixedupdate
    {
        if (travelRoute == null) return; //Waiting for route

        if (!startedTraversing)
        {
            if (travelRoute.Count > 1) travelRoute.Pop(); //Remove Start Vertex (shortestpath returns a list including the start and end vertex)
            startedTraversing = true;
        }

        if (travelRoute.Count > 0) //Still sth left in the stack
        {
            nextVertex = travelRoute.Peek();
            Module nextVertexModule = data.GetModuleFromID(travelRoute.Peek().moduleID);
            if (nextVertexModule != data.IsInModule(this))
            {
                LeaveModule();
                EnterModule(nextVertexModule);
                return;
            }


            UpdateNearbyPersons();
            float distance = Vector3.Distance(travelRoute.Peek().transform.position, transform.position); //Check distance between this Person (transform.position) and the Vertex's position
            if (distance < 3.5f)
            {
                lastVisitedVertex = travelRoute.Pop(); //Pop off the reached Vertex from our travelRoute and assign it to lastVisitedVertex

                //Teststuff
                //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //cube.transform.position = lastVisitedVertex.position;
            }
            if (travelRoute.Count > 0) //If stack still not empty:
            {
                MoveTowards(travelRoute.Peek().transform.position);
            }
            else //Stack is empty -> Person has reached its destination
            {
                startedTraversing = false; //reset for next time

                if (targetBuilding != null)
                {
                    EnterBuilding(targetBuilding);
                }
                else
                {
                    //Start(); //FOR TESTING . LAter for staying somewhere outside for some time                  
                }
            }
        }
    }

    void MoveTowards(Vector3 goal)
    {

        Vector3 direction = (goal - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        WalkInDirectionOf(lookRotation);

        //CARS will get a much simpler seperate DriveInDirectionOf function
    }


    //This person takes care of a pedestrians walking movement. It also talkes care of people evading or overtaking each other while sticking to minimumDistanceregulations
    void WalkInDirectionOf(Quaternion lookRotation)
    {
        Quaternion evasiveRotation = Quaternion.identity;
        float speedFactor = 1;

        if (nearestPerson != null && Vector3.Angle(transform.forward, travelRoute.Peek().transform.position - transform.position) < 70)
        {
            Vector3 diff = nearestPerson.transform.position - transform.position;

            if (diff.magnitude <= RestrictionMgr.Instance.minimumDistance * 1.5f && Vector3.Dot(transform.forward.normalized, diff.normalized) > 0.3f) //if nearby and clearly in front
            {
                currentSpeed = speed;
                if (Vector3.Dot(transform.forward.normalized, nearestPerson.transform.forward.normalized) > 0.8f && speed <= nearestPerson.currentSpeed * 1.2f)
                {
                    speedFactor = 0.0f;  //Same walking direction as other and now sense in overtaking-> Slow down
                    currentSpeed = nearestPerson.currentSpeed;
                }
                else
                {
                    float immediacyForEvasion = 2f - diff.magnitude / ((RestrictionMgr.Instance.minimumDistance + 0.5f) * 1.5f); //between 1 and 2

                    float angle = VectorMath.GetSignedAngle(transform.forward, nearestPerson.transform.position - transform.position);
                    if (angle > 0) //If other person is on lefthand side
                    {
                        evasiveRotation = Quaternion.AngleAxis(90f * Time.fixedDeltaTime * immediacyForEvasion, Vector3.up); //Turn right to evade

                    }
                    else //If other person is on righthand side 
                    {
                        evasiveRotation = Quaternion.AngleAxis(-90f * Time.fixedDeltaTime * immediacyForEvasion, Vector3.up); //Turn left to evade
                    }
                }
            }
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * turnSpeed) * evasiveRotation; //Final Look Direction
        transform.Translate(Vector3.forward * currentSpeed * speedFactor * Time.fixedDeltaTime); //Forward Movement
    }
    #endregion

    #endregion

    #region Stay Inside Buidling

    private DayTime stayUntillTime;  
    private float stayForTime;
    public void stayInsideBuildingUntil(bool skipQueue = false)
    {
        //Store special case:
        if(data.IsInBuilding(this) is BuildingStore && !skipQueue)
        {
            ((BuildingStore)data.IsInBuilding(this)).QueueUp(this);
        }

        if (stayUntillTime != null) SleepHandlerQueue.SetWakeUpTime(stayUntillTime, this);
        
        else
        {
            DayTime current = new DayTime();
            current.AddMinute(stayForTime);
            SleepHandlerQueue.SetWakeUpTime(current, this);
        }
        gameObject.SetActive(false);
    }

    #endregion

    #region Public Methods for RoutineMgr    

    public void SetUpSocialMeeting()
    {
        //Pick Random Friend Group
        List<PersonBase> friends = data.GetFriends(this);
        PersonBase friend = friends[Random.Range(0, friends.Count)];
        friends = data.GetCommonFriends(this, friend);

        //Sort Out friends who do not have time
        List<PersonBase> confirmed = new List<PersonBase>();
        foreach(PersonBase candidate in friends)
        {
            if (candidate.routineMgr.isAvailableForSocial())
            {
                confirmed.Add(candidate);
            }
        }

        //Remove people if meeting size is restricted
        if (confirmed.Count + 1 > RestrictionMgr.Instance.MeetingSize)
        {
            for(int i = 0; i < (confirmed.Count - RestrictionMgr.Instance.MeetingSize); i++)
            {
                confirmed.RemoveAt(Random.Range(0, confirmed.Count));
            }
        }

        if (confirmed.Count == 0)
        {
            routineMgr.Next(); //If person would be the only one attending
            return;
        }

        //Add person itself
        confirmed.Add(this);

        //Pick Location for Meeting
        BuildingBase location;
        if (Random.Range(0, 4) == 0 || RestrictionMgr.Instance.isGastroClosed()) location = data.GetHome(this);
        else
        {
            location = BuildingSocial.PickNearbySocial(this, confirmed.Count);
            if (location == null) location = data.GetHome(this);
        }

        foreach(PersonBase attendant in confirmed)
        {
            attendant.routineMgr.joinSocial(location);
        }

        data.EstablishMeeting(confirmed);

    }

    //BUGFIXING
    public bool wasInsideBuildingBeforeInterrupt;
    //END

    public BuildingBase meetingLocation = null;
    public void JoinMeetingUntil(BuildingBase location, DayTime untilTime)
    {
        meetingLocation = location; //LeaveBuilding triggers onBuildingExit which potentially needs to know whether person is in meeting location 
        wasInsideBuildingBeforeInterrupt = (updateMode == UpdateMode.INSIDE);
        if (!gameObject.activeInHierarchy) SleepHandlerQueue.WakeEarly(this); //Remove from sleepqueue

        SetStayUntilTime(untilTime);
        if (data.IsInBuilding(this) == location) stayInsideBuildingUntil();
        else
        {
            PlotTo(location);
            if (updateMode == UpdateMode.INSIDE) LeaveBuilding();
        }
        meetingLocation = null; //information no longer needed
    }

    public void GoToStore()
    {
        SetStayUntilTime(null);
        stayForTime = Random.Range(20, 60); //In store sth between 20 and 60 min

        BuildingStore store = BuildingStore.PickNearbyStore(this);
        PlotTo(store);
        if (updateMode == UpdateMode.INSIDE) LeaveBuilding();
    }

    public void GoToHospital()
    {
        //Set Wait in Hospital untill infectionMgr updated cycleEnd
        SetStayUntilTime(infectionMgr.cycleEnd);
        
        BuildingHospital hospital = data.personIsHospitalisedIn(this);

        //Walking there as PLACEHOLDER! (Later Ambulance?)
        PlotTo(hospital);
        if (updateMode == UpdateMode.INSIDE) LeaveBuilding();
    }

    public void BeInHospital()
    {
        //Set Wait in Hospital untill infectionMgr updated cycleEnd
        SetStayUntilTime(infectionMgr.cycleEnd);      

        //Person is physically already in Hospital
        if (data.IsInBuilding(this) == data.personIsHospitalisedIn(this)){
            if (new DayTime() > infectionMgr.cycleEnd) return;
            stayInsideBuildingUntil();
        }

        //Go sure routine mgr has person stay in hospital until fever or better
    }

    public void SetStayUntilTime(DayTime time)
    {
        stayUntillTime = time;
    }

    public void goHomeUntil(DayTime time)
    {
        SetStayUntilTime(time);
        BuildingBase home = data.GetHome(this);
        if (data.IsInBuilding(this) == home) stayInsideBuildingUntil();
        else
        {
            PlotTo(home);
            if (updateMode == UpdateMode.INSIDE) LeaveBuilding();
        }        
    }

    public void goWorkUntil(DayTime time)
    {
        SetStayUntilTime(time);
        BuildingBase workplace = data.GetWorkplace(this);
        if (data.IsInBuilding(this) == workplace) stayInsideBuildingUntil();
        else
        {
            PlotTo(workplace);
            if (updateMode == UpdateMode.INSIDE) LeaveBuilding();
        }        
    }
    
    #endregion

    //Awake is called when object is instantiated 
    protected virtual void Awake()
    {
        //TEST:
        if (ID % 1000 == 0) print(ID);
        routineMgr = GetComponent<RoutineMgr>();
        infectionMgr = GetComponent<InfectionMgr>();
    }

    //DEBUGGING
    public GameObject home;
    public GameObject office;


    //Awake is called when object is instantiated
    protected virtual void Start()
    {
        #region tests
        infectionMgr.TestSetup();
        speed = Random.Range(3f, 6f);
        turnSpeed = speed * 0.5f;
        currentSpeed = speed;

        Module currentModule = data.buildingIsInModule(data.GetHome(this));
        EnterModule(currentModule);
        routineMgr.InitializeRoutine();

        //Precompute Key-Routes
        networkGraph.queueRoute(this, networkGraph.GetBuildingVertex(data.GetHome(this).ID), networkGraph.GetBuildingVertex(data.GetWorkplace(this).ID));
        waitingForRoute += 1;


        //DEBUGGING
        home = data.GetHome(this).gameObject;
        office = data.GetWorkplace(this).gameObject;

        //Set animator
        animator = GetComponent<Animator>();
        animator.speed = speed * 0.5f;

        #endregion
    }

    protected virtual void Update()
    {
        if(routeHomeOffice == null && travelRoute == null) //Has not gotten assigned this route yet
        {
            return;
        }
        else if(routeHomeOffice == null && travelRoute != null)
        {
            routeHomeOffice = new List<Vertex>(travelRoute); //Move received travelRoute to routeHomeOffice
            travelRoute = null;
        }

        if (!Alive) print("update despite dead!?");

        if (updateMode == UpdateMode.CITYOUTSIDE) UpdateTravelStatus();
        else if (updateMode == UpdateMode.INSIDE) routineMgr.Next();


        //DEBUGGING:
        if (travelRoute != null) displayableRouteForDebugging = travelRoute.ToList<Vertex>();
        else displayableRouteForDebugging = null;
        if (prevTravelRoute != null) prevTravelRouteForDebugging = prevTravelRoute.ToList<Vertex>();
        else prevTravelRouteForDebugging = null;
    }
    protected virtual void FixedUpdate()
    {
        if (!Alive) print("update despite dead!?");

        if (updateMode == UpdateMode.MODULEOUTSIDE)
        {
            TraverseRoute();
            GetComponent<InfectionMgr>().UpdateAllExposuresOutside(); //TESTING
        }
    }
}
