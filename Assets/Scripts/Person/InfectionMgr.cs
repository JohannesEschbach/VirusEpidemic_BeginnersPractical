using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class InfectionMgr : MonoBehaviour
{
    private static Data data = Data.Instance;
    private PersonBase thisPerson;
    private static System.Random r = new System.Random();

    public bool confirmedInfected { get; set; } = false;
    public bool infected { get; private set; } = false;
    public bool justGotInfected; //Race condition issues...I dont remember why this is necessary
    public bool vaccinated = false;
    public bool wantsVaccination = false;
    public int daysSinceRecovery = -1;
    private int cyclesBeforeImprovement = Virus.cyclesBeforeImprovement;
    public DayTime cycleEnd;
    public DayTime testingCycleEnd;
    public Symptom stageOfIllness { get; private set; }
    public float testWillingness; //1 as standard

    private Immune immuneSystem;

    /// <value> Dictionary <c>exposedTo</c> tracks exposure. <br/>
    /// Key: Infected nearby person <br/>
    /// Value: Accumulated infection chance</value>
    public Dictionary<PersonBase, float> exposedTo = new Dictionary<PersonBase, float>();

    private static void CheckAndRefreshAllExposure(InfectionMgr personInfMgr)
    {
        lock (threadLocker2)
        {
            foreach (KeyValuePair<PersonBase, float> pair in personInfMgr.exposedTo)
            {
                if (personInfMgr.DetermineIfInfected(pair.Key)) break;
            }
            personInfMgr.exposedTo.Clear();
        }
    }

    /// <summary>Method <c>DetermineIfInfected</c> determines if this person caught the virus from an infected other person </summary>
    protected bool DetermineIfInfected(PersonBase otherPerson)
    {        
        float exposure = exposedTo[otherPerson];        
        if (r.NextDouble() <= exposure)
        {
            justGotInfected = true;


            //bugfixing-balancing
            BuildingBase building = data.IsInBuilding(thisPerson);
            if (building != null)
            {
                if (building is BuildingHome) numHomeInf += 1;
                else if (building is BuildingOffice) numOfficeInf += 1;
                else if (building is BuildingSocial) numSocialInf += 1;
                else if (building is BuildingStore) numStoreInf += 1;
            }
            else
            {
                if (data.IsInModule(thisPerson).IsFocused())
                {
                    numModuleOutsideInf += 1;
                }
                else numCityOutsideInf += 1;
            }


            return true;
        }
        return false;
    }

    private void GetInfected()
    {
        if (infected) return; //Failsafe ... this really shouldnt happen

        lock (threadLocker2)
        {
           exposedTo.Clear();
        }

        cycleEnd = new DayTime();
        cycleEnd.AddDay(Virus.symptomChangeCycleLength);  

        infected = true;
        cyclesBeforeImprovement = Virus.cyclesBeforeImprovement;

        Stats.currentInfected++;
        if (!data.isAlreadyInfected(thisPerson))
        {
            data.setAlreadyInfected(thisPerson);
            Stats.addTotalInfected();
        }
    }

    private void Recover()
    {
        daysSinceRecovery = 0;
        infected = false;
        cyclesBeforeImprovement = Virus.cyclesBeforeImprovement; //reset
        
        Stats.currentInfected--;
        Stats.recovered++;
        if (confirmedInfected)
        {
            Stats.confirmedCurrentInfected--;
            Testing.consumeTest();
        }
        confirmedInfected = false;


        DayTime testTime = new DayTime();
        testTime.AddDay(1);
        testingCycleEnd = testTime;
    }

    private void UpdateStageOfIllness()
    {
        float chanceOfWorsening = Virus.GetWorsenChance(stageOfIllness, immuneSystem, vaccinated, daysSinceRecovery, data.personIsHospitalisedIn(thisPerson) != null);
        double roll = r.NextDouble();
        if (roll <= chanceOfWorsening && cyclesBeforeImprovement == Virus.cyclesBeforeImprovement)
        {
            if ((int)stageOfIllness == Symptom.GetNames(typeof(Symptom)).Length - 1)
            {
                thisPerson.OnDeath();
                return; 
            }
            else if(stageOfIllness < Symptom.COLD && roll <= chanceOfWorsening / 4) stageOfIllness = (Symptom)((int)stageOfIllness + 2); //Skip stage
            else stageOfIllness = (Symptom)((int)stageOfIllness + 1);
            if (stageOfIllness >= Symptom.RTILIGHT)
            {
                //ROUTINE MGR GO TO HOSPITAL
                if(!confirmedInfected)Testing.ConfirmInfection(thisPerson);
                thisPerson.GetComponent<RoutineMgr>().beInHospital();
            }
        }
        else
        {
            if (cyclesBeforeImprovement == 0)
            {
                if(stageOfIllness > 0) stageOfIllness = (Symptom)((int)stageOfIllness-1);
                if(stageOfIllness < Symptom.RTILIGHT)
                {
                    if(data.personIsHospitalisedIn(thisPerson) != null)
                    {
                        data.personIsHospitalisedIn(thisPerson).LeaveHospital(thisPerson);
                        thisPerson.GetComponent<RoutineMgr>().goHome();
                    }
                }
                if (stageOfIllness == Symptom.NONE) {
                    Recover();
                    return;
                }               
            }
            else cyclesBeforeImprovement--;
        }
        cycleEnd.AddDay(Virus.symptomChangeCycleLength);
    }

    public static IEnumerator UpdateCoursesOfDisease() 
    {
        while (true)
        {
            yield return new WaitForSeconds(DayTime.scaledTimeToUnityTime(60f)); //once per ingame hour

            DayTime dayTime = new DayTime();

            for(int i = 0; i < data.NumOfPeople(); i++)
            {
                PersonBase person = data.GetPersonFromID(i);
                if (!person.Alive) continue;
                InfectionMgr infmgr = person.infectionMgr;
                if (infmgr.infected)
                {
                    if (infmgr.cycleEnd < dayTime)
                    {
                        infmgr.UpdateStageOfIllness();
                    }
                }
                if(infmgr.testingCycleEnd < dayTime)
                {
                    if (person.infectionMgr.daysSinceRecovery != -1) person.infectionMgr.daysSinceRecovery++;

                    //Tests for suspected Cases and Person is suspected case (coughs or worse)
                    if (infmgr.stageOfIllness >= Symptom.COUGH && !infmgr.confirmedInfected && Testing.availability > 0)
                    {
                        Testing.TestCheck(person);
                    }
                    //Tests for everyone
                    else if (!infmgr.confirmedInfected && Testing.availability > 1)
                    {
                        Testing.TestCheck(person);
                    }
                    if (infmgr.confirmedInfected) person.GetComponent<RoutineMgr>().goHome();

                    infmgr.testingCycleEnd.AddDay(1);
                }
            }
        }
    }

    #region Outside

    public List<PersonBase> nearbyInfected = new List<PersonBase>(); //Each update we need to check if someone new is around or not around anymore. Therefore we compare this (always up to date) nearbyInfected List with our exposedTo Dictionary

    ///<summary>Method <c>UpdateAllExposuresOutside</c> updates exposure to nearby infected persons based on proximity, time, and their symptoms. (OUTSIDE BUILDING ONLY!) </summary>
    public void UpdateAllExposuresOutside()
    {
        try
        {
            if (!infected)
            {
                foreach (PersonBase otherPerson in exposedTo.Keys.ToList())
                {
                    if (nearbyInfected.Contains(otherPerson)) //Person from exposedTo is still around
                    {
                        float distance = Vector3.Distance(otherPerson.gameObject.transform.position, transform.position);
                        Symptom otherPersonWorstSymptom = otherPerson.infectionMgr.stageOfIllness;

                        float exposure = Virus.CalculateExposure(DayTime.scale * Time.deltaTime, distance, otherPersonWorstSymptom, 1, vaccinated, daysSinceRecovery, RestrictionMgr.Instance.maskRules == 2);
                        exposedTo[otherPerson] += exposure / 2; //update exposure to this other person //divided by two for gameplay time appropriateness (people walking too slow compared to time scale)
                    }
                    else //Person from exposedTo is not around anymore
                    {
                        {
                            if (DetermineIfInfected(otherPerson)) exposedTo.Clear();
                        }
                    }
                }                
            }
        }
        catch
        {
            //Race Conditions can happen here when a person enters a module just before it got focused on by the player. 
            //The background thread will then wipe the exposed-To Dictionary at one point while real-time tracking is already active.
            //This is really neglegible, as it happens only very occasionally and the consequences are insignificant
            
            print("Race Condition ouch!");
        };

    }

    #endregion

    #region BackgroundChecker

    private static Queue<System.Tuple<PersonBase, Module, float, float, bool>> leaveModuleExposureCheckQueue = new Queue<System.Tuple<PersonBase, Module, float, float, bool>>();
    private static Queue<System.Tuple<PersonBase, Module, float, float, bool>> leaveModuleExposureCheckQueueSinceSnapshot = new Queue<System.Tuple<PersonBase, Module, float, float, bool>>();

    private static Thread leaveModuleExposureChecker = null;
    private static object threadLocker = new System.Object();
    public static object threadLocker2 = new System.Object();


    private static Dictionary<int, List<PersonBase>> personsOutsidePresentInModule;
    private static Dictionary<int, Dictionary<PersonBase, float>> personsPresentEntryTimeModule;



    public static IEnumerator copyDictionaries()
    {
        while (true)
        {
            if (leaveModuleExposureCheckQueueSinceSnapshot.Count == 0)
            {
                lock (threadLocker)
                {
                    personsOutsidePresentInModule = new Dictionary<int, List<PersonBase>>();
                    foreach (KeyValuePair<int, List<int>> pair in data.personsOutsidePresentInModule)
                    {
                        personsOutsidePresentInModule[pair.Key] = data.GetPersonFromID(pair.Value);
                    }

                    personsPresentEntryTimeModule = new Dictionary<int, Dictionary<PersonBase, float>>();
                    foreach (KeyValuePair<int, Dictionary<int, float>> pair in data.personsPresentEntryTimeModule)
                    {
                        personsPresentEntryTimeModule[pair.Key] = new Dictionary<PersonBase, float>();
                        foreach (KeyValuePair<int, float> entry in pair.Value)
                        {
                            PersonBase person = data.GetPersonFromID(entry.Key);
                            personsPresentEntryTimeModule[pair.Key][person] = entry.Value;
                        }
                    }

                    leaveModuleExposureCheckQueueSinceSnapshot = leaveModuleExposureCheckQueue;
                    leaveModuleExposureCheckQueue = new Queue<System.Tuple<PersonBase, Module, float, float, bool>>();
                }                   
                yield return new WaitForSeconds/*Realtime*/(10f * DayTime.scale); //Neglegible that someone could be exactly 10 ingame min in module and out of module again
            }
        }
    }

    static void IterateAllPersonsOutside(PersonBase person, Module module, float entryTime, float exitTime, bool isModuleOutsideMode)
    {
        InfectionMgr personInfMgr = person.infectionMgr;
        List<PersonBase> peopleOutsidePresent = personsOutsidePresentInModule[module.ID];
        foreach (PersonBase otherPerson in peopleOutsidePresent)
        {
            if (otherPerson == person) continue;
            if (isModuleOutsideMode && otherPerson.GetUpdateMode() == UpdateMode.MODULEOUTSIDE) continue; //Race conditions neglegible here...not noticable on large scale unless someone switches module focus like a madman
            InfectionMgr otherPersonInfMgr = otherPerson.infectionMgr;
            if ((personInfMgr.infected && !otherPersonInfMgr.infected) || (!personInfMgr.infected && otherPersonInfMgr.infected))
            {
                personsPresentEntryTimeModule[module.ID].TryGetValue(otherPerson, out float otherEntry);
                if (otherEntry == 0.0f) continue;                           //THIS SHOULDNT BE AN ISSUE, I DONT KNOW WHY IT IS AN ISSUE, BUT I REALLY SHOULD KNOW...
                float lastEntry = Mathf.Max(entryTime, otherEntry);
                if (lastEntry >= exitTime)
                {
                    continue;
                }//If other person entered after this person left (could happen due to multithreading solution)
                float timeExposed = exitTime - lastEntry;

                float chance = (timeExposed / (300f / 4.5f)) / 4; //MAGIC NUM: 300 is average time to move through module

                float exposureTime = (r.NextDouble() < chance) ? 5f : 0f; //MAGIC NUMBER: 5 seconds when walking past each other

                PersonBase healthyPers = otherPerson;
                PersonBase infPers = person;             
                    
                if(!personInfMgr.infected)
                {
                    healthyPers = person;
                    infPers = otherPerson;
                }

                float exposure = Virus.CalculateExposure(exposureTime, RestrictionMgr.Instance.minimumDistance, infPers.infectionMgr.stageOfIllness, 1, healthyPers.infectionMgr.vaccinated, healthyPers.infectionMgr.daysSinceRecovery, RestrictionMgr.Instance.maskRules == 2); 
                lock (threadLocker2)
                {
                    if (healthyPers.infectionMgr.exposedTo.ContainsKey(infPers)) healthyPers.infectionMgr.exposedTo[infPers] = exposure;
                    else healthyPers.infectionMgr.exposedTo.Add(infPers, exposure);
                }
            }
        }
        CheckAndRefreshAllExposure(personInfMgr);
    } 

    static void leaveModuleExposureCheckerLoop() 
    {
        while (true)
        {
            if (leaveModuleExposureCheckQueueSinceSnapshot.Count > 0)
            {
                lock (threadLocker)
                {
                    System.Tuple<PersonBase, Module, float, float, bool> item = leaveModuleExposureCheckQueueSinceSnapshot.Dequeue();
                    if (item == null) continue;
                    PersonBase person = item.Item1;
                    Module module = item.Item2;
                    float entryTime = item.Item3;
                    float exitTime = item.Item4;
                    bool isModuleOutsideMode = item.Item5;

                    IterateAllPersonsOutside(person, module, entryTime, exitTime, isModuleOutsideMode);                   
                }
            }
        }
    }

    public void LeaveModuleExposureCheck()
    {
        Module module = data.IsInModule(thisPerson);

        leaveModuleExposureCheckQueue.Enqueue(new System.Tuple<PersonBase, Module, float, float, bool>(thisPerson, module, data.PersonEnteredModuleWhen(module, thisPerson), Time.time, thisPerson.GetUpdateMode() == UpdateMode.MODULEOUTSIDE));
    }

    public void LeaveModuleExposureCheckMainThread(Module module)
    {
        //List<PersonBase> peopleOutsidePresent = personsOutsidePresentInModule[module.ID];
        float entryTime = data.PersonEnteredModuleWhen(module, thisPerson);

        IterateAllPersonsOutside(this.thisPerson, module, entryTime, Time.time, thisPerson.GetUpdateMode() == UpdateMode.MODULEOUTSIDE);

    }

    #region Inside

    /// <value> float <c>sameRoomDistance</c> sets how far people would sit apart in the same room. <br/>
    //private float sameRoomDistance = 2.5f; //TEST VALUE, MIGHT CHANGE LATER

    ///<summary>Method <c>LeaveBuildingInfectedList</c> is called when this Person leaves a building. It sets this Person's final exposedTo values (if this person is not infected) or fills out other Persons' values (if this person is infected). It also updates whether this person infected themselves or not (INSIDE BUILDING ONLY!) </summary>
    public void LeaveBuildingExposureCheck()
    {
        BuildingBase building = data.IsInBuilding(thisPerson);
        if (building.GetType() == typeof(BuildingHospital)) return; //No regular tracking for hospitals

        foreach (PersonBase otherPerson in data.PersonsPresentInBuilding(building))
        {
            InfectionMgr otherPersonInfMgr = otherPerson.GetComponent<InfectionMgr>();
            if (otherPerson == this.thisPerson) continue;

            if ((infected && !otherPersonInfMgr.infected) || (!infected && otherPersonInfMgr.infected))
            {

                float lastEntry = Mathf.Max(data.PersonEnteredBuildingWhen(building, thisPerson), data.PersonEnteredBuildingWhen(building, otherPerson));
                float timeExposed = Time.time - lastEntry;

                PersonBase healthyPers = otherPerson;
                PersonBase infPers = thisPerson;
                if (!infected)
                {
                    healthyPers = thisPerson;
                    infPers = otherPerson;
                }
                float exposure = building.GetIndoorExposure(infPers, healthyPers, timeExposed);
                lock (threadLocker2)
                {
                    if (healthyPers.infectionMgr.exposedTo.ContainsKey(infPers)) healthyPers.infectionMgr.exposedTo[infPers] = exposure;
                    else healthyPers.infectionMgr.exposedTo.Add(infPers, exposure);

                }
            }      
        }
        CheckAndRefreshAllExposure(this);
    }

    #endregion
    #endregion

    public void Awake()
    {
        if(leaveModuleExposureChecker == null)
        {
            leaveModuleExposureChecker = new Thread(leaveModuleExposureCheckerLoop);
            leaveModuleExposureChecker.Start();
        }
        cyclesBeforeImprovement = Virus.cyclesBeforeImprovement;
        wantsVaccination = (Random.Range(0f, 1f) < Vaccine.populationVaccineWillingness);
        testWillingness = Random.Range(0.2f, 1.8f);
        DayTime testTime = new DayTime();
        testTime.AddMinute(Random.Range(60f, 23f * 60f));
        testingCycleEnd = testTime;
        stageOfIllness = Symptom.NONE;
        if (Random.Range(0f, 1f) < Virus.populationPercentRiskGroup) immuneSystem = Immune.COMORBIDITIES;
        else immuneSystem = (Immune)Random.Range(1, 3);
    }

    public void Update()
    {
        if (justGotInfected)
        {
            GetInfected();
            justGotInfected = false;
        }

}

    public void TestSetup()
    {
        thisPerson = GetComponent<PersonBase>();
        //immuneSystem = (Immune)Random.Range(0, Symptom.GetNames(typeof(Symptom)).Length);
        immuneSystem = Immune.MEDIUM;

        //TESTING
        if (thisPerson.ID % (int)(data.NumOfPeople()/Populator.numZeroCases) == 0)
        {
            GetInfected();
            stageOfIllness = Symptom.COLD;
        }        
    }

    //DEBUGGING
    public static int numCityOutsideInf = 0;
    public static int numModuleOutsideInf = 0;
    public static int numHomeInf = 0;
    public static int numOfficeInf = 0;
    public static int numSocialInf = 0;
    public static int numStoreInf = 0;

}
