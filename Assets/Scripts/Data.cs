using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Data
{
    private static Data instance;
    private Data() { }
    public static Data Instance
    {
        get
        {
            if (instance == null) instance = new Data();
            return instance;
        }
    }

    #region Person
    /// <value> Property <c>iDToPersons</c> maps a Person ID to the Person</value>
    private Dictionary<int, PersonBase> iDToPerson = new Dictionary<int, PersonBase>();
    /// <value> Property <c>friendMap</c> maps a Person ID to a list of friend IDs they are friends with</value>
    private Dictionary<int, List<int>> friendMap = new Dictionary<int, List<int>>();
    /// <value> Property <c>householdMap</c> maps a Person ID to a list of Person IDs they live together with in the same flat</value>
    private Dictionary<int, List<int>> householdMap = new Dictionary<int, List<int>>();
    /// <value> Property <c>coworkersMap</c> maps a Person ID to a list of Person IDs they work together with in the same room</value>
    private Dictionary<int, List<int>> coworkersMap = new Dictionary<int, List<int>>();
    /// <value> Property <c>personToBuilding</c> maps a Person ID to the Building ID they are currently in </value>
    private Dictionary<int, int> personToBuilding = new Dictionary<int, int>();
    /// <value> Property <c>personToHome</c> maps a Person ID to the Building ID of their home </value>
    private Dictionary<int, int> personToHome = new Dictionary<int, int>();
    /// <value> Property <c>personToWorkplace</c> maps a Person ID to the Building ID of their workplace </value>
    private Dictionary<int, int> personToWorkplace = new Dictionary<int, int>();
    /// <value> Property <c>personToModule</c> maps a Person ID to the Module ID they are currently in </value>
    private Dictionary<int, int> personToModule = new Dictionary<int, int>();

    private Dictionary<int, List<int>> personIsMeetingWith = new Dictionary<int, List<int>>();

    private Dictionary<int, int> personToHospital = new Dictionary<int, int>();
    private Dictionary<int, bool> iDAlreadyInfected = new Dictionary<int, bool>();
    private Dictionary<int, bool> iDAlreadyConfirmedInfected = new Dictionary<int, bool>();

    public bool HasConfirmedInfectedHouseholdMember(PersonBase person)
    {
        foreach (PersonBase member in GetHousehold(person))
        {
            if (member.infectionMgr.confirmedInfected) return true;
        }
        return false;
    }

    public List<PersonBase> GetCommonFriends(PersonBase a, PersonBase b)
    {
        return GetFriends(a).Intersect(GetFriends(b)).ToList();
    }

    public List<PersonBase> PersonIsMeetingWith(PersonBase person)
    {
        return GetPersonFromID(personIsMeetingWith[person.ID]);
    }

    public void EstablishMeeting(List<PersonBase> persons)
    {
        foreach (PersonBase person in persons)
        {
            foreach (PersonBase otherPerson in persons)
            {
                if (otherPerson != person) personIsMeetingWith[person.ID].Add(otherPerson.ID);
            }
        }
    }

    public void LeaveMeeting(PersonBase person)
    {
        List<PersonBase> inMeetingWith = PersonIsMeetingWith(person);
        foreach (PersonBase otherPerson in inMeetingWith)
        {
            PersonIsMeetingWith(otherPerson).Remove(person);
        }
        inMeetingWith.Clear();
    }

    public void hospitalisePerson(PersonBase person, BuildingHospital hospital)
    {
        personToHospital[person.ID] = hospital.ID;
        patients[hospital.ID].Add(person.ID);
    }

    public BuildingHospital personIsHospitalisedIn(PersonBase person)
    {
        if (!personToHospital.ContainsKey(person.ID)) return null;
        return (BuildingHospital)GetBuildingFromID(personToHospital[person.ID]);
    }

    public void leaveHospital(PersonBase person)
    {
        BuildingHospital hospital = personIsHospitalisedIn(person);
        personToHospital.Remove(person.ID);
        patients[hospital.ID].Remove(person.ID);
    }

    public void setAlreadyInfected(PersonBase person)
    {
        iDAlreadyInfected[person.ID] = true;
    }

    public bool isAlreadyInfected(PersonBase person)
    {
        return iDAlreadyInfected[person.ID];
    }

    public void setAlreadyConfirmedInfected(PersonBase person)
    {
        iDAlreadyConfirmedInfected[person.ID] = true;
    }

    public bool isAlreadyConfirmedInfected(PersonBase person)
    {
        return iDAlreadyConfirmedInfected[person.ID];
    }

    public int NumOfPeople()
    {
        return iDToPerson.Count;
    }

    public void AddPerson(PersonBase person)
    {
        person.ID = PersonBase.nextID;
        PersonBase.nextID++;

        iDToPerson.Add(person.ID, person);
        friendMap[person.ID] = new List<int>();
        householdMap[person.ID] = new List<int>();
        coworkersMap[person.ID] = new List<int>();
        personIsMeetingWith[person.ID] = new List<int>();
        iDAlreadyInfected[person.ID] = false;
        iDAlreadyConfirmedInfected[person.ID] = false;

    }

    public void SetWorkplace(PersonBase person, BuildingBase office)
    {
        personToWorkplace[person.ID] = office.ID;
        employees[office.ID].Add(person.ID);
    }

    public BuildingBase GetWorkplace(PersonBase person)
    {
        return GetBuildingFromID(personToWorkplace[person.ID]);
    }

    public void SetHome(PersonBase person, BuildingHome home)
    {
        personToHome[person.ID] = home.ID;
        residents[home.ID].Add(person.ID);
    }

    public void AddHousehold(List<PersonBase> persons)
    {
        foreach (PersonBase person in persons)
        {
            foreach (PersonBase member in persons)
            {
                if (member != person) householdMap[person.ID].Add(member.ID);
            }
        }
    }

    public void AddCoworkersMap(List<PersonBase> persons)
    {
        foreach (PersonBase person in persons)
        {
            foreach (PersonBase member in persons)
            {
                if (member != person) coworkersMap[person.ID].Add(member.ID);
            }
        }
    }

    public PersonBase GetPersonFromID(int iD)
    {
        return iDToPerson[iD];
    }

    public List<PersonBase> GetPersonFromID(List<int> iDs)
    {
        List<PersonBase> persons = new List<PersonBase>();
        foreach (int iD in iDs) persons.Add(GetPersonFromID(iD));
        return persons;
    }

    public void AddFriendGroup(List<PersonBase> friends)
    {
        foreach(PersonBase person in friends)
        {
            List<PersonBase> friendsWithoutPerson = new List<PersonBase>(friends);
            friendsWithoutPerson.Remove(person);
            AddFriends(person, friendsWithoutPerson);
        }
    }

    public void AddFriends(PersonBase person, List<PersonBase> friends)
    {
        List<int> friendIDs = new List<int>();
        foreach(PersonBase friend in friends)
        {
            friendIDs.Add(friend.ID);
        }
        friendMap[person.ID].AddRange(friendIDs);
    }

    public List<PersonBase> GetFriends(PersonBase person)
    {
        return GetPersonFromID(friendMap[person.ID]);
    }

    public List<PersonBase> GetHousehold(PersonBase person)
    {
        return GetPersonFromID(householdMap[person.ID]);
    }

    public List<PersonBase> GetCoworkers(PersonBase person)
    {
        return GetPersonFromID(coworkersMap[person.ID]);
    }

    public BuildingBase GetHome(PersonBase person)
    {
        return GetBuildingFromID(personToHome[person.ID]);
    }

    public BuildingBase IsInBuilding(PersonBase person)
    {
        if (!personToBuilding.ContainsKey(person.ID)) return null;        
        return GetBuildingFromID(personToBuilding[person.ID]);
    }

    public Module IsInModule(PersonBase person)
    {
        personToModule.TryGetValue(person.ID, out int moduleID);
        return GetModuleFromID(moduleID);
    }

    #endregion

    #region Buildings
    /// <value> Property <c>iDToBuilding</c> maps a Building ID to the Building</value>
    private Dictionary<int, BuildingBase> iDToBuilding = new Dictionary<int, BuildingBase>();
    /// <value> Property <c>personsPresentInBuilding</c> maps a Building ID to the list of person IDs present </value>
    private Dictionary<int, List<int>> personsPresentInBuilding = new Dictionary<int, List<int>>();
    private Dictionary<int, Dictionary<int, float>> personsPresentEntryTimeBuilding = new Dictionary<int, Dictionary<int, float>>();
    private Dictionary<int, int> buildingToModule = new Dictionary<int, int>();
    private Dictionary<int, List<int>> residents = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> employees = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> patients = new Dictionary<int, List<int>>();

    private List<int> homes = new List<int>();
    private List<int> offices = new List<int>();
    private List<int> hospitals = new List<int>();
    private List<int> stores = new List<int>();
    private List<int> socials = new List<int>();

    public List<PersonBase> GetHospitalPatients(BuildingHospital hospital)
    {
        return GetPersonFromID(patients[hospital.ID]);
    }

    public List<BuildingSocial> GetSocials()
    {
        return GetBuildingFromID(socials).Cast<BuildingSocial>().ToList();
    }

    public List<BuildingStore> GetStores()
    {
        return GetBuildingFromID(stores).Cast<BuildingStore>().ToList();
    }

    public List<BuildingHospital> GetHospitals()
    {
        return GetBuildingFromID(hospitals).Cast<BuildingHospital>().ToList();
    }

    public List<BuildingHome> GetHomes()
    {
        return GetBuildingFromID(homes).Cast<BuildingHome>().ToList();
    }

    public List<BuildingOffice> GetOffices()
    {
        return GetBuildingFromID(offices).Cast<BuildingOffice>().ToList(); ;
    }

    public List<PersonBase> GetEmployees(BuildingBase building)
    {
        return GetPersonFromID(employees[building.ID]);
    }

    public List<PersonBase> GetResidents(BuildingHome home)
    {
        return GetPersonFromID(residents[home.ID]);
    }

    public Module buildingIsInModule(BuildingBase building)
    {
        buildingToModule.TryGetValue(building.ID, out int moduleID);
        return GetModuleFromID(moduleID);
    }
    public void AddBuilding(BuildingBase building)
    {
        iDToBuilding.Add(building.ID, building);
        personsPresentInBuilding[building.ID] = new List<int>();
        personsPresentEntryTimeBuilding[building.ID] = new Dictionary<int, float>();     

        if (building is BuildingHome)
        {
            residents[building.ID] = new List<int>();
            homes.Add(building.ID);
        }
        else
        {
            employees[building.ID] = new List<int>();
        }
        if (building is BuildingOffice) offices.Add(building.ID);
        else if (building is BuildingHospital)
        {
            hospitals.Add(building.ID);
            patients[building.ID] = new List<int>();
        }
        else if (building is BuildingStore) stores.Add(building.ID);
        else if (building is BuildingSocial) socials.Add(building.ID);
    }

    public BuildingBase GetBuildingFromID(int iD)
    {
        return iDToBuilding[iD];
    }

    public List<BuildingBase> GetBuildingFromID(List<int> iDs)
    {
        List<BuildingBase> buildings = new List<BuildingBase>();
        foreach (int iD in iDs) buildings.Add(GetBuildingFromID(iD));
        return buildings;
    }


    public List<PersonBase> PersonsPresentInBuilding(BuildingBase building)
    {
        return GetPersonFromID(personsPresentInBuilding[building.ID]);
    }


    public void AddPersonToBuilding(PersonBase person, BuildingBase building)
    {
        personToBuilding[person.ID] = building.ID;
        personsPresentInBuilding[building.ID].Add(person.ID);
        //Also add to Timetracker:
        SetPersonEntryTimeBuilding(person, building);
    }

    public void RemovePersonFromBuilding(PersonBase person)
    {
        BuildingBase building = IsInBuilding(person);
        personToBuilding.Remove(person.ID);
        personsPresentInBuilding[building.ID].Remove(person.ID);
        //Also remove from Timetracker:
        RemovePersonEntryTimeBuilding(person, building);
    }

    public float PersonEnteredBuildingWhen(BuildingBase building, PersonBase person)
    {
        Dictionary<int, float> personsWithEntryTime = personsPresentEntryTimeBuilding[building.ID];
        personsWithEntryTime.TryGetValue(person.ID, out float time);
        return time;
    }

    private void SetPersonEntryTimeBuilding(PersonBase person, BuildingBase building)
    {
        personsPresentEntryTimeBuilding[building.ID][person.ID] = Time.time;
    }

    private void RemovePersonEntryTimeBuilding(PersonBase person, BuildingBase building)
    {
        personsPresentEntryTimeBuilding[building.ID].Remove(person.ID);
    }

    #endregion

    #region Modules
    /// <value> Property <c>iDToBuilding</c> maps a Module ID to the Module</value>
    private Dictionary<int, Module> iDToModule = new Dictionary<int, Module>();
    /// <value> Property <c>personsPresentInModule</c> maps a Module ID to the list of person IDs present </value>
    private Dictionary<int, List<int>> personsPresentInModule = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> personsOutsidePresentInModule { get; private set; } = new Dictionary<int, List<int>>();
    public Dictionary<int, Dictionary<int, float>> personsPresentEntryTimeModule { get; private set; } = new Dictionary<int, Dictionary<int, float>>(); //Time Person entered the streets of the module. Person "leaves" module when entering building too

    private Dictionary<int, bool> moduleIsHomeOfficeMode = new Dictionary<int, bool>();

    public void toggleModuleHomeOfficeMode(Module module)
    {
        moduleIsHomeOfficeMode[module.ID] = !moduleIsHomeOfficeMode[module.ID];
    }

    public void setModuleHomeOfficeMode(Module module, bool value)
    {
        moduleIsHomeOfficeMode[module.ID] = value;
    }

    public bool isModuleHomeOfficeMode(Module module)
    {
        return moduleIsHomeOfficeMode[module.ID];
    }

    public Module GetModuleFromID(int iD)
    {
        return iDToModule[iD];
    }

    public void AddModule(Module module)
    {
        iDToModule.Add(module.ID, module);
        personsPresentInModule[module.ID] = new List<int>();
        personsPresentEntryTimeModule[module.ID] = new Dictionary<int, float>();
        personsOutsidePresentInModule[module.ID] = new List<int>();
        moduleIsHomeOfficeMode[module.ID] = false;
    }

    public void AddPersonToModule(PersonBase person, Module module, bool outside)
    {
        personToModule[person.ID] = module.ID;
        personsPresentInModule[module.ID].Add(person.ID);
        if (outside) AddPersonToModuleOutside(person, module);
    }

    public void AddPersonToModuleOutside(PersonBase person, Module module)
    {
        personsOutsidePresentInModule[module.ID].Add(person.ID);
        SetPersonEntryTimeModule(person, module);
    }

    public void RemovePersonFromModule(PersonBase person)
    {
        RemovePersonFromModuleOutside(person);
        Module module = IsInModule(person);
        personToModule.Remove(person.ID);
        personsPresentInModule[module.ID].Remove(person.ID);
    }

    public void RemovePersonFromModuleOutside(PersonBase person)
    {
        Module module = IsInModule(person);
        personsOutsidePresentInModule[module.ID].Remove(person.ID);
        RemovePersonEntryTimeModule(person, module);
    }

    public void AddBuildingToModule(BuildingBase building, Module module)
    {
        buildingToModule[building.ID] = module.ID;
    }


    public float PersonEnteredModuleWhen(Module module, PersonBase person)
    {
        Dictionary<int, float> personsWithEntryTime = personsPresentEntryTimeModule[module.ID];
        personsWithEntryTime.TryGetValue(person.ID, out float time);
        return time;
    }

    private void SetPersonEntryTimeModule(PersonBase person, Module module)
    {
        personsPresentEntryTimeModule[module.ID][person.ID] = Time.time;
    }

    private void RemovePersonEntryTimeModule(PersonBase person, Module module)
    {
        personsPresentEntryTimeModule[module.ID].Remove(person.ID);
    }

    public List<PersonBase> PersonsPresentInModule(Module module)
    {
        return GetPersonFromID(personsPresentInModule[module.ID]);
    }

    public List<PersonBase> PersonsOutsidePresentInModule(Module module)
    {
        return GetPersonFromID(personsOutsidePresentInModule[module.ID]);
    }

    #endregion


}
