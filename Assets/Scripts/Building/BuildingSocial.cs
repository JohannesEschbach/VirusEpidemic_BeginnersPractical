using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingSocial : BuildingBase, SupportsColoring, IsWorkplace
{
    public int numOfEmployees;
    private int numClients = 0;
    public static float sameRoomDistance = 0.5f;

    public bool SupportsHomeOffice()
    {
        return false;
    }
    public int GetNumOfEmployees()
    {
        return numOfEmployees;
    }

    public static BuildingSocial PickNearbySocial(PersonBase person, int seats)
    {
        List<BuildingSocial> allSocials = Data.Instance.GetSocials();
        List<BuildingSocial> availableSocials = new List<BuildingSocial>();
        foreach (BuildingSocial social in allSocials)
        {
            if (RestrictionMgr.Instance.ClientsPerSocial - social.numClients >= seats) availableSocials.Add(social);
        }
        if (availableSocials.Count == 0) return null;
        List<BuildingSocial> sortedSocials = availableSocials.OrderBy(social => Vector3.Distance(person.transform.position, social.transform.position)).ToList();
        int i = Random.Range(0, Mathf.Min(5, sortedSocials.Count));
        return sortedSocials[i];
    }


    public bool IsPersonsInSameRoom(PersonBase a, PersonBase b) //representative for same table
    {
        return data.PersonIsMeetingWith(a).Contains(b);
    }

    public override float GetIndoorExposure(PersonBase infectedPerson, PersonBase otherPerson, float timeExposed) 
    {
        bool sameRoom = this.IsPersonsInSameRoom(infectedPerson, otherPerson);

        float distance;
        int relationshipType;
        if (sameRoom)
        {
            relationshipType = 2;
            distance = sameRoomDistance;
        }

        else
        {
            relationshipType = 1;
            distance = Random.Range(3f, 5f);            
        }
        return Virus.CalculateExposure(timeExposed, distance, infectedPerson.infectionMgr.stageOfIllness, relationshipType, otherPerson.infectionMgr.vaccinated, otherPerson.infectionMgr.daysSinceRecovery, RestrictionMgr.Instance.maskRules > 0); //In room with coworker/friend/household        
    }

    public override void OnBuildingExit(PersonBase person)
    {
        numClients--;
        if (data.PersonIsMeetingWith(person).Count != 0)
        {
            data.LeaveMeeting(person);
        }
    }

    public override void OnBuildingEnter(PersonBase person)
    {
        numClients++;
    }

    public List<PersonBase> GetPeopleForAverageInfection()
    {
        return data.PersonsPresentInBuilding(this);
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
    }
}
