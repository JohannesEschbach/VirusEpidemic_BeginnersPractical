using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIStatDisplay : MonoBehaviour
{

    public Text TotalInfected;
    public Text CurrentInfected;
    public Text Fatalities;
    public Text Recovered;
    public Text TestsPerHour;
    public Text HospitalSpaces;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TotalInfected.text = "Total Infected: " + Stats.totalInfected.ToString();
        CurrentInfected.text = "Current Infected: " + Stats.currentInfected.ToString();
        Fatalities.text = "Fatalities: " + Stats.fatalities.ToString();
        Recovered.text = "Recovered: " + Stats.recovered.ToString();
        TestsPerHour.text = Testing.testsLastHour.ToString() + " Tests/Hour";
        HospitalSpaces.text = "Hospital Beds: " + BuildingHospital.NumOfFreeBeds().ToString();
    }
}
