using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Virus
{
    //Move this to a vaccine class!!!
    public static float vaccineEffec = 0.9f;     //THIS IS ODD?... is it 90% u cant get infected at all, or every time a dice roll when you would get infected
    public static float maskEffec = 0.2f;
    public static float immunityDetoriationPerDay = 0.05f; //0.05 normally e.g.

    //Some default values here (can be set in Scenario design). Infection multipliers.
    public static Dictionary<Symptom, float> symptomMultiplier = new Dictionary<Symptom, float>()
    {
        {Symptom.NONE, 1 },
        {Symptom.COLD, 3 },     
        {Symptom.COUGH, 10 },
        {Symptom.FEVER, 15 },
        {Symptom.RTILIGHT, 20 },
        {Symptom.RTIHEAVY, 20 }
    };

    //Some default values here (can be set in Scenario design)
    public static Dictionary<int, float> relationshipMultiplier = new Dictionary<int, float>()
    {
        {1, 1f}, //Base
        {2, 5f}, //Friend
        {3, 4f} //Household
    };

    public static Dictionary<Immune, float> worsenChanceMultiplier = new Dictionary<Immune, float>()
    {
        {Immune.COMORBIDITIES, 2f},
        {Immune.WEAK, 1.2f},
        {Immune.MEDIUM, 1f},
        {Immune.STRONG, 0.8f},
    };

    public static float infectiveness = 1;
    public static float populationPercentRiskGroup = 0.05f;
    public static float vaccineWorseningMod = 0.5f;
    public static float hospitalizedWorseningMod = 0.5f;
    public static float worsenChanceMod = 1f;
    public static int cyclesBeforeImprovement = 1;
    public static int symptomChangeCycleLength = 1;
    public static List<float> worsenChance = new List<float> { 2f, 1f, 0.5f, 0.2f, 0.5f, 1f };

    public static float GetWorsenChance(Symptom stage, Immune immuneSystem, bool vaccinated = false, int daysSinceRecovery = -1, bool isHospitalized = false)
    {
        float multiplier = 1;

        //CHECK IF PERSON IS HOSPITALIZED AND RESPECTIVLY IMPROVE CHANCES!!
        if (vaccinated) multiplier *= vaccineWorseningMod;         
        if (daysSinceRecovery != -1) multiplier *= Mathf.Min((immunityDetoriationPerDay * daysSinceRecovery), 1);
        if (isHospitalized) multiplier *= hospitalizedWorseningMod;
        return worsenChanceMod * worsenChance[(int)stage] * worsenChanceMultiplier[immuneSystem] * multiplier;
    }



    ///<summary>Method <c>distanceExposureFunction</c> returns the infection chance per second (in-game Minute outside) for a given distance </summary>
    public static float DistanceExposureFunction(float distance)
    {
        float exposure = infectiveness * (1f / (0.3f * (distance + 0.15f)) + 0.05f) / 600f; //? normally  //Looks like sensible exposure per minute (inGame Hour) for now when plotted. CAN BE SET IN SCENARIO DESIGN
        return exposure / 60f; //per ingame Minute outside)
    }

    /// <summary>Method <c>UpdateExposure</c> updates exposure to an infected person based on proximity, time, and their symptoms.</summary>
    public static float CalculateExposure(float inGameMin, float distance, Symptom worstSymptom, int relationshipType, bool vaccinated, int daysSinceRecovery, bool hasMask)
    {
        inGameMin = inGameMin * DayTime.scale;  //MAKE THE HOLE THING DAYTIME BASED!!!
        float distanceExposurePerMinute = DistanceExposureFunction(distance);
        float infectChance = inGameMin * distanceExposurePerMinute;

        float multiplier = 1f;
        if (hasMask) multiplier *= maskEffec;
        if (vaccinated) multiplier *= (1 - vaccineEffec);
        if (daysSinceRecovery != -1) multiplier *= Mathf.Min((immunityDetoriationPerDay * daysSinceRecovery), 1);
        return infectChance * symptomMultiplier[worstSymptom] * relationshipMultiplier[relationshipType] * multiplier;
    }
}
