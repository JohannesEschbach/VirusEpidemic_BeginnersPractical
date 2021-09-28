using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableSetter : MonoBehaviour
{
    public Incrementer citySize;
    public Incrementer cliqueSize;
    public Incrementer percHomeOffice;
    public Incrementer cliquePerPers;

    public Incrementer fatality;
    public Incrementer infectiveness;
    public Incrementer percRiskGroup;
    public Incrementer zeroCases;

    public Incrementer vaccWill;
    public Incrementer daysUntilAvailable;
    public Incrementer daysUntilSupplied;
    public Incrementer vaccEff;


    public void onPlay() 
    {
        Populator.numOfFriendGroups = (int)cliquePerPers.value;
        Populator.sizeOFriendGroups = (int)cliqueSize.value;
        BuildingOffice.percHomeOfficeCapable = (int)percHomeOffice.value;
        CityBuilder.modulesPerRow = (int)citySize.value + 1;

        Virus.worsenChance[3] *= fatality.value / 5;  //5% as default
        Virus.infectiveness = infectiveness.value;
        Virus.populationPercentRiskGroup = percRiskGroup.value/100f;
        Populator.numZeroCases = (int)zeroCases.value;

        Vaccine.populationVaccineWillingness = vaccWill.value/100f;
        Vaccine.daysUntillResearched = (int)daysUntilAvailable.value;
        Vaccine.daysFullySupplied = (int)daysUntilSupplied.value;
        Virus.vaccineEffec = vaccEff.value/100f;
        Virus.vaccineWorseningMod = Mathf.Pow(1 - (float)vaccEff.value, 1/4); //Intention that its spreaded out over first 4 symptom changes
    }


}
