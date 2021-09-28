using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOfInfectionShower : MonoBehaviour
{


    public int CityOutsideInf = 0;
    public int ModuleOutsideInf = 0;
    public int HomeInf = 0;
    public int OfficeInf = 0;
    public int SocialInf = 0;
    public int StoreInf = 0;

    // Update is called once per frame
    void Update()
    {
        //Debugging!!:
        CityOutsideInf = InfectionMgr.numCityOutsideInf;
        ModuleOutsideInf = InfectionMgr.numModuleOutsideInf;
        HomeInf = InfectionMgr.numHomeInf;
        OfficeInf = InfectionMgr.numOfficeInf;
        SocialInf = InfectionMgr.numSocialInf;
        StoreInf = InfectionMgr.numStoreInf;
    }
}
