using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingStore : BuildingBase, SupportsColoring, IsWorkplace
{
    public bool isEssential;

    public int numOfEmployees;
    private int numClients = 0;
    private Queue<PersonBase> queue = new Queue<PersonBase>();
    public static float sameRoomDistance = 1f;

    public bool SupportsHomeOffice()
    {
        return false;
    }
    public int GetNumOfEmployees()
    {
        return numOfEmployees;
    }

    public static BuildingStore PickNearbyStore(PersonBase person)
    {
        List<BuildingStore> allStores = Data.Instance.GetStores();
                
        List<BuildingStore> sortedStores = allStores.OrderBy(store => Vector3.Distance(person.transform.position, store.transform.position)).ToList();
        int i = Random.Range(0, 3);
        return sortedStores[i];
    }
    
    public void QueueUp(PersonBase person)
    {
        if(numClients < RestrictionMgr.Instance.ClientsPerStore)
        {
            person.stayInsideBuildingUntil(true);
            numClients++;
        }

        else
        {
            data.RemovePersonFromBuilding(person); //Remove for queuing from building
            data.AddPersonToModuleOutside(person, data.IsInModule(person)); //Add again to module outside for time queuing
            person.SetUpdateMode(UpdateMode.MODULEOUTSIDE);
            person.gameObject.SetActive(false);
            queue.Enqueue(person);
        }
        
    }
    

    public override float GetIndoorExposure(PersonBase infectedPerson, PersonBase otherPerson, float timeExposed)
    {
        //Chance of standing next to the person:
        float chanceOfMeeting = 1 / 3f;
        timeExposed = 0;
        if (Random.Range(0, 1) < chanceOfMeeting)
        {
            timeExposed = 3; //3 Min each
        }        
        return Virus.CalculateExposure(timeExposed, Mathf.Max(sameRoomDistance, RestrictionMgr.Instance.minimumDistance), infectedPerson.infectionMgr.stageOfIllness, 1, otherPerson.infectionMgr.vaccinated, otherPerson.infectionMgr.daysSinceRecovery, RestrictionMgr.Instance.maskRules > 0);
    }

    public List<PersonBase> GetPeopleForAverageInfection()
    {
        return data.PersonsPresentInBuilding(this);
    }

    public override void OnBuildingExit(PersonBase person)
    {
        numClients--;
    }

    public override void OnBuildingEnter(PersonBase person)
    {       
        //numFreeClients-- no need to implement here, is done via update!
    }

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();        
    }

    protected override void Update()
    {
        base.Update();

        if(numClients < RestrictionMgr.Instance.ClientsPerStore && queue.Count > 0)
        {
            PersonBase person = queue.Dequeue();
            data.RemovePersonFromModuleOutside(person); //Add again from module outside after queuing
            person.infectionMgr.LeaveModuleExposureCheck(); //THIS is ofc an ugly solution and maybe needs some reworking in future. People in queue are not considerd closer to another than to other people in the module
            data.AddPersonToBuilding(person, this); //Add person back to building after queueing
            numClients++;
            person.SetUpdateMode(UpdateMode.INSIDE);
            person.stayInsideBuildingUntil(true);
        }
    }
}
