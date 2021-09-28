using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingOffice : BuildingBase, SupportsColoring, IsWorkplace
{
    public int numOfRooms;

    public int minNrOfPeoplePerRoom;
    public int maxNrOfPeoplePerRoom;

    public float sameRoomDistance = 1.5f;

    public static int percHomeOfficeCapable = 100;
    private bool supportsHomeOffice;

    public bool SupportsHomeOffice()
    {
        return supportsHomeOffice;
    }

    public int GetNumOfEmployees()
    {
        List<PersonBase> employees = data.GetEmployees(this);
        if (employees == null) return numOfRooms * (maxNrOfPeoplePerRoom + minNrOfPeoplePerRoom) / 2; //In case if not yet assigned give approximation ... Bad OOP!! :/
        else return employees.Count;
    }

    private bool IsPersonsInSameRoom(PersonBase a, PersonBase b)
    {
        return data.GetCoworkers(a).Contains(b);
    }

    public override float GetIndoorExposure(PersonBase infectedPerson, PersonBase otherPerson, float timeExposed)
    {        
        bool sameRoom = this.IsPersonsInSameRoom(infectedPerson, otherPerson);
        int relationshipType = 1;

        float distance = Mathf.Max(sameRoomDistance, RestrictionMgr.Instance.minimumDistance);

        float exposure;
        if (sameRoom)
        {
            //print(timeExposed * DayTime.scale);
            exposure = Virus.CalculateExposure(timeExposed, distance, infectedPerson.infectionMgr.stageOfIllness, relationshipType, otherPerson.infectionMgr.vaccinated, otherPerson.infectionMgr.daysSinceRecovery, RestrictionMgr.Instance.maskRules > 0); //In room with coworker/friend/household
        }

        else
        {
            //Chance of meeting that person in the staircase:
            float chanceOfMeeting = 1 / 3.5f; //Will be approx. twice per week
            timeExposed = 0;
            if (Random.Range(0, 1) < chanceOfMeeting)
            {
                timeExposed = 10; //10 min smalltalk or meeting or queue in cateria etc... 
            }
            exposure = Virus.CalculateExposure(timeExposed, distance, infectedPerson.infectionMgr.stageOfIllness, relationshipType, otherPerson.infectionMgr.vaccinated, otherPerson.infectionMgr.daysSinceRecovery, RestrictionMgr.Instance.maskRules > 0); // 1/300 of time in same room with some random person (hallways etc.). This value might change later.
        }
        return exposure;
    }

    public List<PersonBase> GetPeopleForAverageInfection()
    {
        return data.PersonsPresentInBuilding(this);
    }

    protected override void Awake()
    {
        base.Awake();
        supportsHomeOffice = Random.Range(0, 100) <= percHomeOfficeCapable;
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
