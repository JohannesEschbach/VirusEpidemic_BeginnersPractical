using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Stats
{
    public static int populationBeginning = 0; //get that number right after creating the city
    public static int currentInfected = 0;
    public static int confirmedCurrentInfected = 0;
    public static int fatalities = 0;
    public static int recovered = 0;
    public static int totalInfected = 0;
    public static int confirmedTotalInfected = 0;
    public static int numPeopleVaccinated = 0;

    public static float budget = 1f; //value between 0 and 1
    public static float happiness = 1f; //value between 0 and 1     

    public static bool updateHappiness(List<float> contributions)
    {
        happiness = contributions.Sum();
        if(happiness < 0)
        {
            happiness = 0;
            return false;
        }
        return true;
    }

    public static void updateBudget(List<float> contributions, float budgetRecovery)
    {
        budget = Mathf.Min(1f, budget - (contributions.Sum() - budgetRecovery));        
    }

    public static void addTotalInfected()
    {
        totalInfected++;
    }    

    public static int currentPopulation()
    {
        return populationBeginning - fatalities;
    }
}
