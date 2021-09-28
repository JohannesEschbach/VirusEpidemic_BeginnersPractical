using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingHospital : BuildingBase, IsWorkplace
{
    //These values should probably be adequately set based on hospitals/population
    public static int avgNumOfEmployees = 0;
    public static int avgNumOfSlots = 0;

    public int numOfEmployees;
    public int numOfSlots;
    private float workingTogetherDistance = 10f;  //4 because of good hygienic measures

    private bool isFunctional = true;

    public bool SupportsHomeOffice()
    {
        return false;
    }

    public int GetNumOfEmployees()
    {
        return numOfEmployees; 
    }

    //Hospital only functional when min 20% employees healthy!! IMPLEMENT THAT
    private IEnumerator CheckIfHospitalFunctional()
    {
        while (true)
        {
            List<PersonBase> employees = data.GetEmployees(this);
            int numSick = 0;
            foreach(PersonBase employee in employees)
            {
                if(employee.infectionMgr.confirmedInfected | employee.infectionMgr.stageOfIllness >= Symptom.FEVER)
                {
                    numSick++;
                }
            }
            if(isFunctional && numSick/(float)numOfEmployees > 0.2f)
            {
                numOfSlots = 0;
                TryRelocate();
            }
            else if(!isFunctional && numSick / (float)numOfEmployees <= 0.2f)
            {
                numOfSlots = avgNumOfSlots;
            }

            yield return new WaitForSeconds(DayTime.scaledTimeToUnityTime(20f));
        }
    }

    private void TryRelocate()
    {
        List<PersonBase> patients = data.GetHospitalPatients(this);
        foreach(PersonBase patient in patients)
        {
            data.leaveHospital(patient);
            patient.GetComponent<RoutineMgr>().beInHospital();
        }
    }

    public static int NumOfFreeBeds()
    {
        int count = 0;
        List<BuildingHospital> allHospitals = Data.Instance.GetHospitals();
        foreach(BuildingHospital hospital in allHospitals)
        {
            count += hospital.numOfSlots;
        }
        return count;
    }

    public static BuildingHospital RegisterAtHospital(PersonBase person)
    {
        BuildingHospital hospital = FindNearestAvailableHospital(person);
        if (hospital == null) return null;
        Data.Instance.hospitalisePerson(person, hospital);
        return hospital;
    }

    public static BuildingHospital FindNearestAvailableHospital(PersonBase person)
    {
        List<BuildingHospital> allHospitals = Data.Instance.GetHospitals();
        List<BuildingHospital> sortedHospitals = allHospitals.OrderBy(hospital => Vector3.Distance(person.transform.position, hospital.transform.position)).ToList();
        foreach(BuildingHospital hospital in sortedHospitals)
        {
            if(hospital.numOfSlots > 0)
            {
                hospital.numOfSlots--;
                return hospital;
            }
        }
        return null;
    }

    /*public bool assignICU(PersonBase person)
    {
        if(numOfICU > 0)
        {
            numOfICU--;
            return true;
        }
        else
        {
            //Maybe instead of transporting patient better move ICUs around..

            //IMPLEMENT PROPERLY LATER!!!
            return false;
            /*
            List<BuildingHospital> sortedHospitals = allHospitals.OrderBy(hospital => Vector3.Distance(this.transform.position, hospital.transform.position)).ToList();
            foreach(BuildingHospital hospital in sortedHospitals)
            {
                if(hospital.numOfICU > 0)
                {
                    data.leaveHospital(person);
                    data.hospitalisePerson(person, hospital);
                    hospital.numOfICU--;

                    //Move person to new hospital...idk maybe implement once we have ambulances... we gotta at least have ambulances ... :/ But thats the last thing to do...For now they will teleport i guess
                    
                    //TELEPORTATION: Implementation (careful! Leave Module, Leave Building Enter Module Enter Building etc....)
                                        
                }
            }            
        }
        return false;
    }*/

    public override float GetIndoorExposure(PersonBase infectedPerson, PersonBase otherPerson, float timeExposed)
    {
        //Assumption: Working together with a different person every half an hour.
        float workingTogetherUnits = timeExposed/30f;       
        float chanceOfWorkingTogether = workingTogetherUnits * 1/numOfEmployees;
        timeExposed = 0;
        if (Random.Range(0, 1) < chanceOfWorkingTogether)
        {
            timeExposed = 30;
        }
        return Virus.CalculateExposure(timeExposed, workingTogetherDistance, infectedPerson.infectionMgr.stageOfIllness, 1, otherPerson.infectionMgr.vaccinated, otherPerson.infectionMgr.daysSinceRecovery, true); 
    }

    public void LeaveHospital(PersonBase person)
    {
        numOfSlots++;
        data.leaveHospital(person);
    }

    public override void OnBuildingEnter(PersonBase person)
    {
        //NO NEED TO IMPLEMENT STH HERE!
    }

    protected override void Awake()
    {
        base.Awake();
        //allHospitals.Add(this);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        StartCoroutine(CheckIfHospitalFunctional());
    }

    protected override void Update()
    {
        base.Update();
    }
}
