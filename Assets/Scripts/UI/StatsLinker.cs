using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsLinker : MonoBehaviour
{
    //HOW THE FUCK DO I GET A MUTABLE INTEGER IN C#


    public UILineRenderer currentInfected;
    public UILineRenderer confirmedCurrentInfected;
    public UILineRenderer vaccinated;
    public UILineRenderer totalInfected;
    public UILineRenderer confirmedTotalInfected;
    public UILineRenderer deaths;
    public UILineRenderer recoveries;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentInfected.plottedValue = Stats.currentInfected;
        confirmedCurrentInfected.plottedValue = Stats.confirmedCurrentInfected;
        totalInfected.plottedValue = Stats.totalInfected;
        confirmedTotalInfected.plottedValue = Stats.confirmedTotalInfected;
        vaccinated.plottedValue = 100 * Stats.numPeopleVaccinated / Data.Instance.NumOfPeople();
        deaths.plottedValue = (Stats.fatalities + Stats.recovered) != 0 ? 100 * Stats.fatalities / (Stats.fatalities + Stats.recovered) : 0;
        recoveries.plottedValue = (Stats.fatalities + Stats.recovered) != 0 ? 100 * Stats.recovered / (Stats.fatalities + Stats.recovered) : 100;
    }
}
