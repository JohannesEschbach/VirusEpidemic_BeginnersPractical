using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHome : BuildingBase, SupportsColoring
{
    public int minNumFlatmates;
    public int maxNumFlatmates;
    public int numOfFlats;

    public float sameRoomDistance = 3f;

    private bool IsPersonsInSameRoom(PersonBase a, PersonBase b)
    {

        if(data.GetHousehold(a).Contains(b)) return true;
        else
        {
            foreach(PersonBase friend in data.PersonIsMeetingWith(a))
            {
                if (data.GetHousehold(friend).Contains(b)) return true;
            }
            return false;
        }
    }
   
    public override float GetIndoorExposure(PersonBase infectedPerson, PersonBase otherPerson, float timeExposed)
    {        
        bool sameRoom = this.IsPersonsInSameRoom(infectedPerson, otherPerson);
        int relationshipType = 1;
        float distance = sameRoomDistance;

        if (sameRoom)
        {
            if (data.GetHousehold(infectedPerson).Contains(otherPerson)) relationshipType = 3;
            if (data.PersonIsMeetingWith(infectedPerson).Contains(otherPerson))
            {
                relationshipType = 2;
                distance = BuildingSocial.sameRoomDistance;
            }
        }

        else
        {
            //Chance of meeting that person in the staircase: Approximately once per week per person
            float chanceOfMeeting = 1 / 7f;
            timeExposed = 0;
            if(Random.Range(0,1) < chanceOfMeeting)
            {
                timeExposed = 3; //3 Min smalltalk etc.
            }
            distance = 2f;
        }

        return Virus.CalculateExposure(timeExposed, distance, infectedPerson.infectionMgr.stageOfIllness, 
            relationshipType, otherPerson.infectionMgr.vaccinated, otherPerson.infectionMgr.daysSinceRecovery, false);
    }

    public override void OnBuildingExit(PersonBase person)
    {
        if(person.meetingLocation == this)
        {
            data.LeaveMeeting(person);
        }
    }

    public List<PersonBase> GetPeopleForAverageInfection()
    {
        return data.GetResidents(this);
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
