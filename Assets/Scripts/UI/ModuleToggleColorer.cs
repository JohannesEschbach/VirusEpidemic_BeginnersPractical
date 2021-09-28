using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModuleToggleColorer : MonoBehaviour
{
    private Toggle toggle;
    private Image img;
    private Module module;
    private const int maxRGB = 255;
    private bool clicked;

    private float GetAverageInfection(List<PersonBase> personsPresent)
    {
        if (personsPresent == null || personsPresent.Count == 0) { print("emptyModule"); return 0; }
        int cumulatedValue = 0;
        int counter = 0;
        foreach (PersonBase person in personsPresent)
        {
            InfectionMgr infectionMgr = person.GetComponent<InfectionMgr>();
            if (infectionMgr.infected)
            {
                cumulatedValue += 1; // (int)infectionMgr.stageOfIllness;
            }
            counter++;
        }
        float avg = (float)cumulatedValue / counter;
        return avg;
    }

    void UpdateColor()
    {
        float avgInfection = GetAverageInfection(Data.Instance.PersonsPresentInModule(module));
        int decrease = (int)(maxRGB * Mathf.Clamp(5f * avgInfection, 0 , 1)); // Factor 2 for better visibility         
        
        if (!clicked)
        {            
            img.color = new Color32(maxRGB, (byte)(int)Mathf.Clamp((maxRGB - decrease), 0, maxRGB), (byte)(int)Mathf.Clamp((maxRGB - decrease), 0, maxRGB), (int)(0.6 * maxRGB));
        }
        else
        {            
            img.color = new Color32((int)(0.7 * maxRGB), (byte)(int)(0.7 * Mathf.Clamp((maxRGB - decrease), 0, maxRGB)), (byte)(0.7 * (int)Mathf.Clamp((maxRGB - decrease), 0, maxRGB)), (int)(0.6 * maxRGB));
        }
    }


    IEnumerator Start()
    {
        toggle = GetComponent<Toggle>();
        img = GetComponent<Image>();

        ModuleButton moduleButton = GetComponent<ModuleButton>();
        module = CityBuilder.GetModuleByIndex(moduleButton.x, moduleButton.y);
        toggle.group = GameObject.Find("CityModulesPanel").GetComponent<ToggleGroup>();

        while (true)
        {
            UpdateColor();
            yield return new WaitForSeconds(2f);
        }

    }

    public void OnToggleValueChanged(bool isOn)
    {
        clicked = isOn;
        UpdateColor();

    }
}
