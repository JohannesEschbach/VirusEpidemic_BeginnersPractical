using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutineMgr : MonoBehaviour
{
    private Data data = Data.Instance;
    private PersonBase person;
    private DayTime goWorkTime;
    private DayTime goHomeTime; //latest Time to go home. Right now only works with people going home before 0:00
    private int hoursOfWork;
    private DayTime lastSocialMeeting;
    public Routine currentRoutine;

    public int freqOfSocialMeeting = 1; //Meet every 2nd day

    private bool isQuarantined()
    {
        return person.infectionMgr.confirmedInfected || data.HasConfirmedInfectedHouseholdMember(person);
    }

    private bool hasTimeBeforeGoHome(float minutes)
    {
        DayTime time = new DayTime();
        time.AddMinute(minutes);
        return !(time.after(goHomeTime));
    }

    private DayTime clampTimeToGoHomeTime(DayTime time)
    {
        DayTime now = new DayTime();
        if(now.day >= time.day) //same Day
        {
            if (time.after(goHomeTime))
            {
                goHomeTime.day = now.day;
                return goHomeTime;
            }
            else return time;
        }
        else //time is tomorrow
        {
            goHomeTime.day = now.day;
            return goHomeTime;
        }
    }

    private void goWork()
    {
        goWorkTime.AddDay(1); //Unless its weekend?
        currentRoutine = Routine.WORK;        
        DayTime finishWorkTime = new DayTime();
        finishWorkTime.AddMinute(hoursOfWork * 60); //Between 6-9h of work including way to workplace
        finishWorkTime = clampTimeToGoHomeTime(finishWorkTime);
        if (RestrictionMgr.Instance.isHomeOfficeMode(person)) person.goHomeUntil(finishWorkTime);
        else person.goWorkUntil(finishWorkTime);
    }

    public void goHome()
    {
        currentRoutine = Routine.HOME;        
        person.goHomeUntil(goWorkTime);
    }

    private void stayHome()
    {
        currentRoutine = Routine.HOME;
        goWorkTime.AddDay(1); //check again next day
        person.goHomeUntil(goWorkTime);
    }

    public void beInHospital()
    {
        if(data.personIsHospitalisedIn(person) == null)
        {
            BuildingHospital hospital = BuildingHospital.RegisterAtHospital(person);
            if(hospital != null)
            {
                currentRoutine = Routine.HOSPITAL;  
                person.GoToHospital();                
            }
            else
            {
                stayHome();
            }
        }
        else
        {
            person.BeInHospital();            
        }
    }

    private void goShopping()
    {
        if (!hasTimeBeforeGoHome(180)) goHome(); //Plan in 3hours for shopping including travel
        currentRoutine = Routine.SHOP; //I am not sure if we need to make restriction of pp in stores -> we would make queues
        person.GoToStore();
    }

    private bool tryStartSocial()
    {
        //i guess that one is the first one to be restricted so it can be make quite simple, hapinness goes down 
        //Budget goes down when Restaurants closed 
        if (((new DayTime() - lastSocialMeeting) / (60 * 24)) >= freqOfSocialMeeting) //Hasnt been to a meeting for the last 3 days
        {
            if (hasTimeBeforeGoHome(240)) //4h planned
            {
                lastSocialMeeting = new DayTime();
                lastSocialMeeting.AddDay(-(freqOfSocialMeeting-1)); //Wait min 1 day before trying again if it doesnt work
                person.SetUpSocialMeeting();
                return true;
            }
        }
        return false;

    }

    public void joinSocial(BuildingBase location)
    {
        currentRoutine = Routine.SOCIAL;
        lastSocialMeeting = new DayTime();
        DayTime stayUntil = new DayTime();
        stayUntil.AddMinute(60 * Random.Range(4, 6));
        stayUntil = clampTimeToGoHomeTime(stayUntil);
        person.JoinMeetingUntil(location, stayUntil);
    }

    public bool isAvailableForSocial()
    {
        //MAYBE HAVE WORK TIME FACTOR IN TOO? SO PEOPLE DONT MEET IN EARLY MORNING

        if (currentRoutine != Routine.HOSPITAL && person.Alive && person.infectionMgr.stageOfIllness < Symptom.FEVER && !isQuarantined())
        {
            if((new DayTime() - lastSocialMeeting)/(60*24) >= freqOfSocialMeeting) //Hasnt been to a meeting for the last 3 days
            {
                if (hasTimeBeforeGoHome(180)) //3h planned
                {
                    return true;
                }
            }            
        }
        return false;
    }

    //IF RESTRICTIONS: work online for office jobs/schools
    //IF RESTRICTIONS: meeting cancellations, restaurants/cafes closed

    public void Next()
    {
        if (currentRoutine == Routine.HOSPITAL || person.infectionMgr.stageOfIllness > Symptom.FEVER) beInHospital();
        else if (person.infectionMgr.stageOfIllness >= Symptom.FEVER) stayHome();
        else if (currentRoutine == Routine.HOME)
        {
            if (isQuarantined()) stayHome();
            else goWork();
        }
        else if (currentRoutine == Routine.WORK)
        {
            if (!tryStartSocial())
            {
                if (Random.Range(0, 2) == 0 | !hasTimeBeforeGoHome(120)) goHome();
                else goShopping();
            }
        }
        else if (currentRoutine == Routine.SHOP)
        {
            if (!tryStartSocial()) goHome();
        }
        else if (currentRoutine == Routine.SOCIAL)
        {
            if (Random.Range(0, 2) == 0 | !hasTimeBeforeGoHome(120)) goHome();
            else goShopping();
        }

    }
    
    public void InitializeRoutine()
    {
        person.SetStayUntilTime(goWorkTime);
        person.EnterBuilding(Data.Instance.GetHome(person));
    }

    private void Awake()
    {
        person = GetComponent<PersonBase>();

        goWorkTime = new DayTime(Random.Range(0, 9), Random.Range(5.0f, 59.9f)); //Between 4:00 and 10:59
        goHomeTime = new DayTime(goWorkTime);
        goHomeTime.AddMinute(Random.Range(12, 14) * 60);
        hoursOfWork = Random.Range(7, 10); //Between 7-9h of work including way to workplace
        lastSocialMeeting = new DayTime();
        lastSocialMeeting.AddDay(Random.Range(-freqOfSocialMeeting + 1, 0));

        currentRoutine = Routine.HOME;
    }

    void Start()
    {
    }

    void Update()
    {
        
    }
}
