using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vaccine : MonoBehaviour
{
    public static int daysUntillResearched = 12;
    public static float researchProgress = 0;

    public static int daysFullySupplied = 20;
    public static int availableDoses = 0;

    public static float populationVaccineWillingness = 0.7f;

    private int stockDailyProduction()
    {
        DayTime time = new DayTime();
        int day = time.day;
        return (int)(Mathf.Pow(Mathf.Sqrt(3) * Mathf.Pow(daysFullySupplied, -1.5f) * (day-daysUntillResearched), 2) * Data.Instance.NumOfPeople());
    }

    private void vaccinatePeople()
    {
        for(int ID = 0; ID < Data.Instance.NumOfPeople() & availableDoses > 0; ID++)
        {            
            InfectionMgr infMgr = Data.Instance.GetPersonFromID(ID).infectionMgr;
            if (!infMgr.vaccinated && infMgr.wantsVaccination)
            {
                infMgr.vaccinated = true;
                availableDoses--;
                Stats.numPeopleVaccinated++;
            }
        }

    }

    IEnumerator updateResearchAndProductionProgress()
    {
        while (researchProgress < 1)
        {
            yield return new WaitForSeconds(DayTime.scaledTimeToUnityTime(60f));
            DayTime time = new DayTime();
            researchProgress = time.getMinutes() / (daysUntillResearched * 24 * 60);
        }
        while (true)
        {
            availableDoses += stockDailyProduction();
            vaccinatePeople();
            yield return new WaitForSeconds(DayTime.scaledTimeToUnityTime(60f * 24));
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(updateResearchAndProductionProgress());

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
