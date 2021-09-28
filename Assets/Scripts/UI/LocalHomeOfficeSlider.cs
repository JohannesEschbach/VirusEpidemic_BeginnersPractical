using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalHomeOfficeSlider : MonoBehaviour
{
    public Slider slider;
    public Slider globalHomeOfficeSlider;
    public void updateGlobalSlider()
    {
        RestrictionMgr restMgr = RestrictionMgr.Instance;
        if (restMgr.numModulesInHomeOffice < (CityBuilder.modulesPerRow * CityBuilder.modulesPerRow) && globalHomeOfficeSlider.value != 0) //CREATE NUMOFMODULES in CITYBUILDER!!
        {
            globalHomeOfficeSlider.SetValueWithoutNotify(0);
        }
    }

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void Update()
    {
        Module focusedModule = Module.focusedModule;
        if (focusedModule == null) return;
        slider.value = (float)System.Convert.ToInt32(Data.Instance.isModuleHomeOfficeMode(focusedModule)); 
    }
}
