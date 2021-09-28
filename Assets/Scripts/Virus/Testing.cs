using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public static int availability = 0; //SET WITH SLIDER
    public static int testsProducedPerDay = 0;
    public static float testFalseNegativeRate = 0.1f;
    public static float costPerHoundredTests = 0.02f;
    
    private static float testsPerHour = 0;
    public static float testsLastHour = 0;

    public void SetAvailability(float value)
    {
        availability = (int)value;
    }

    //Test willingness per day
    public static Dictionary<Symptom, float> testLikelihood = new Dictionary<Symptom, float>()
    {
        {Symptom.NONE, 0.05f },
        {Symptom.COLD, 0.2f },
        {Symptom.COUGH, 0.8f },
        {Symptom.FEVER, 2f },
        {Symptom.RTILIGHT, 10f },
        {Symptom.RTIHEAVY, 10f }
    };

    public static bool TestCheck(PersonBase person)
    {         
        if (Random.Range(0f, 1f) < testLikelihood[person.infectionMgr.stageOfIllness] * person.infectionMgr.testWillingness){
            consumeTest();
            if (person.infectionMgr.infected)
            {
                if (Random.Range(0f, 1f) < testFalseNegativeRate) return false;
                ConfirmInfection(person);
                return true;
            }                        
        }
        return false;
    }

    public static void ConfirmInfection(PersonBase person)
    {
        Stats.confirmedCurrentInfected++;
        Data data = Data.Instance;
        if (!data.isAlreadyConfirmedInfected(person))
        {
            data.setAlreadyConfirmedInfected(person);
            Stats.confirmedTotalInfected++;
        }
        person.infectionMgr.confirmedInfected = true;
    }    

    /*public static void RecoveryTest()
    {
        consumeTest();
        return true;
    }*/

    public static void consumeTest()
    {
        Stats.budget -= costPerHoundredTests / 100;
        testsPerHour++;
    }

    IEnumerator updateResearchAndProductionProgress()
    {
        while (true)
        {            
            yield return new WaitForSeconds(DayTime.scaledTimeToUnityTime(60f));
            testsLastHour = testsPerHour;
            testsPerHour = 0;
        }
    }

    private void Awake()
    {
        StartCoroutine(updateResearchAndProductionProgress());
    }
}
