using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Populator : MonoBehaviour
{

    public GameObject personPrefab;
    private Data data = Data.Instance;

    public static int sizeOFriendGroups = 5;
    public static int numOfFriendGroups = 4;
    public static int numZeroCases = 10;

    //List of All People
    List<PersonBase> persons = new List<PersonBase>();
    //Index k used for all employment positions
    private int k = 0;

    private void AssignWorkersToOneRoomWorkplace<T>(List<T> buildings) where T : BuildingBase, IsWorkplace
    {
        buildings = buildings.OrderBy(x => Random.value).ToList(); //Shuffle
        foreach (T building in buildings)
        {
            if (k >= buildings.Count) break;

            List<PersonBase> coworkers = new List<PersonBase>();
            for (int j = 0; j < building.GetNumOfEmployees(); j++)
            {
                if (k >= buildings.Count) break;
                PersonBase person = persons[k];
                coworkers.Add(person);
                data.SetWorkplace(person, building);

                person.gameObject.SetActive(true);
                k++;
            }
            data.AddCoworkersMap(coworkers);
        }
    }

    public void Populate(List<BuildingHome> homes, List<BuildingOffice> offices, List<BuildingHospital> hospitals, List<BuildingStore> stores, List<BuildingSocial> socials)
    {

        foreach (BuildingHome home in homes)
        {
            for (int i = 0; i < home.numOfFlats; i++)
            {
                int numOfFlatmates = Random.Range(home.minNumFlatmates, home.maxNumFlatmates + 1);
                List<PersonBase> household = new List<PersonBase>();
                for (int j = 0; j < numOfFlatmates; j++)
                {
                    PersonBase person = Instantiate(personPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<PersonBase>();
                    data.AddPerson(person);
                    persons.Add(person);
                    data.SetHome(person, home);
                    household.Add(person);
                }
                data.AddHousehold(household);
            }
        }

        Stats.populationBeginning = persons.Count;        


        persons = persons.OrderBy(x => Random.value).ToList(); //Shuffle

        //Assign Friends

        //Local Friends
        Dictionary<int, List<PersonBase>> peopleFromModule = new Dictionary<int, List<PersonBase>>();        
        foreach(PersonBase person in persons)
        {
            BuildingBase home = data.GetHome(person);
            Module module = data.buildingIsInModule(home);
            if(!peopleFromModule.ContainsKey(module.ID))
            {
                peopleFromModule[module.ID] = new List<PersonBase>();
            }
            peopleFromModule[module.ID].Add(person);
        }        
        foreach(KeyValuePair<int,List<PersonBase>> pair in peopleFromModule)
        {
            List<PersonBase> residents = pair.Value;
            for(int j = 0; j < numOfFriendGroups/2; j++) //Two local friend groups
            {
                List<PersonBase> residentsShuffled = residents.OrderBy(x => Random.value).ToList(); //Shuffle
                for (int i = 0; i < residents.Count(); i += 5)
                {
                    List<PersonBase> friendGroup = residents.GetRange(i, System.Math.Min(5, residents.Count - i));
                    data.AddFriendGroup(friendGroup);
                }
            }            
        }

        //Global Friends
        for (int j = 0; j < numOfFriendGroups / 2; j++) //Two global friend groups
        {
            persons = persons.OrderBy(x => Random.value).ToList(); //Shuffle
            for (int i = 0; i < persons.Count(); i += 5)
            {
                List<PersonBase> friendGroup = persons.GetRange(i, System.Math.Min(5, persons.Count - i));
                data.AddFriendGroup(friendGroup);
            }
        }

        persons = persons.OrderBy(x => Random.value).ToList(); //Shuffle

        
        //Assign hospital personal
        int numHospitals = hospitals.Count;
        BuildingHospital.avgNumOfEmployees = (int)(persons.Count * 0.02f / numHospitals);
        BuildingHospital.avgNumOfSlots = (int)(persons.Count * 0.02f / numHospitals);
        foreach(BuildingHospital hospital in hospitals)
        {
            hospital.numOfEmployees = BuildingHospital.avgNumOfEmployees;
            hospital.numOfSlots = BuildingHospital.avgNumOfSlots;
        }
        AssignWorkersToOneRoomWorkplace(hospitals);

        //Assign employees to stores
        AssignWorkersToOneRoomWorkplace(stores);

        //Assign employees to socials
        AssignWorkersToOneRoomWorkplace(socials);



        /*
        foreach (BuildingHospital hospital in hospitals)
        {
            if (k >= persons.Count) break;
            hospital.numOfSlots = BuildingHospital.avgNumOfSlots;

            List<PersonBase> coworkers = new List<PersonBase>();
            for (int i = 0; i < BuildingHospital.avgNumOfEmployees; i++)
            {
                if (k >= persons.Count) break;

                hospital.numOfEmployees++;
                PersonBase person = persons[k];
                coworkers.Add(person);
                data.SetWorkplace(person, hospital);

                person.gameObject.SetActive(true);
                k++;
            }
            data.AddCoworkersMap(coworkers);
        }


        //USE AN INTERFACE TO MAKE THIS PROPERLY WTF

        //Assign employees to hospitals
        hospitals = hospitals.OrderBy(x => Random.value).ToList(); //Shuffle
        foreach (BuildingHospital hospital in hospitals)
        {
            if (k >= persons.Count) break;

            List<PersonBase> coworkers = new List<PersonBase>();
            for (int j = 0; j < hospital.numOfEmployees; j++)
            {
                if (k >= persons.Count) break;
                PersonBase person = persons[k];
                coworkers.Add(person);
                data.SetWorkplace(person, hospital);

                person.gameObject.SetActive(true);
                k++;
            }
            data.AddCoworkersMap(coworkers);            
        }


        stores = stores.OrderBy(x => Random.value).ToList(); //Shuffle
        foreach (BuildingStore store in stores)
        {
            if (k >= persons.Count) break;

            List<PersonBase> coworkers = new List<PersonBase>();
            for (int j = 0; j < store.numOfEmployees; j++)
            {
                if (k >= persons.Count) break;
                PersonBase person = persons[k];
                coworkers.Add(person);
                data.SetWorkplace(person, store);

                person.gameObject.SetActive(true);
                k++;
            }
            data.AddCoworkersMap(coworkers);
        }



        socials = socials.OrderBy(x => Random.value).ToList(); //Shuffle
        foreach (BuildingSocial social in socials)
        {
            if (k >= persons.Count) break;

            List<PersonBase> coworkers = new List<PersonBase>();
            for (int j = 0; j < social.numOfEmployees; j++)
            {
                if (k >= persons.Count) break;
                PersonBase person = persons[k];
                coworkers.Add(person);
                data.SetWorkplace(person, social);

                person.gameObject.SetActive(true);
                k++;
            }
            data.AddCoworkersMap(coworkers);
        }*/

        //Create module based sublists to simulate local job preference for remaining people. Every 2nd worker works locally        
        int splitIndex = persons.Count - 5 * (persons.Count - k) / 6;
        Stack<PersonBase> remainingWorkers = new Stack<PersonBase>(persons.GetRange(k, splitIndex - k));
        List<PersonBase> localWorkers = persons.GetRange(splitIndex, persons.Count - splitIndex);


        Dictionary<Module, Stack<PersonBase>> moduleToWorkers = new Dictionary<Module, Stack<PersonBase>>();
        foreach(PersonBase localWorker in localWorkers)
        {
            Module module = data.buildingIsInModule(data.GetHome(localWorker));
            if (!moduleToWorkers.ContainsKey(module)) moduleToWorkers.Add(module, new Stack<PersonBase>());
            moduleToWorkers[module].Push(localWorker);
        }        


        //Assign people to Offices
        offices = offices.OrderBy(x => Random.value).ToList(); //Shuffle
        foreach (BuildingOffice office in offices)
        {
            if (k >= persons.Count) break;
            for (int i = 0; i < office.numOfRooms; i++)
            {
                int numOfCoworkers = Random.Range(office.minNrOfPeoplePerRoom, office.maxNrOfPeoplePerRoom + 1);
                List<PersonBase> coworkers = new List<PersonBase>();

                for (int j = 0; j < numOfCoworkers; j++)
                {
                    if (k >= persons.Count) break;
                    k++;

                    PersonBase person = null;    

                    //Take local worker:
                    if (Random.Range(0f, 1f) < 0.8)
                    {                        
                        Module module = data.buildingIsInModule(office);
                        if (moduleToWorkers[module].Count > 0)
                        {
                            person = moduleToWorkers[module].Pop();                                                        
                            
                            coworkers.Add(person);
                            data.SetWorkplace(person, office);
                            person.gameObject.SetActive(true);

                            continue;
                        }
                    }   
                    
                    //Take other worker
                    if(remainingWorkers.Count > 0)
                    {
                        person = remainingWorkers.Pop();
                        coworkers.Add(person);
                        data.SetWorkplace(person, office);
                        person.gameObject.SetActive(true);
                    }
                    //Take any worker from local workers
                    else
                    {                        
                        foreach (KeyValuePair<Module, Stack<PersonBase>> pair in moduleToWorkers.OrderBy(x => Random.value))
                        {
                            if(pair.Value.Count > 0)
                            {
                                person = moduleToWorkers[pair.Key].Pop();
                                break;
                            }
                        }                           
                    }
                    //if (person == null) break;
                    coworkers.Add(person);
                    data.SetWorkplace(person, office);
                    person.gameObject.SetActive(true);
                }
                data.AddCoworkersMap(coworkers);
            }
        }

        int localemployed = 0;
        foreach(PersonBase person in persons)
        {
            try
            {
                data.GetWorkplace(person);
                if (data.buildingIsInModule(data.GetHome(person)) == data.buildingIsInModule(data.GetWorkplace(person))) localemployed++;
            }
            catch
            {
                print("unemployed!");
            }
        }
        //print("local employed: " + localemployed.ToString());
    }

    private void Awake()
    {
        personPrefab.SetActive(false);
    }
}
