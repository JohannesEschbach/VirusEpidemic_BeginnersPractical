using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingColorMgr : MonoBehaviour
{
    private SupportsColoring thisBuilding;
    private const int maxRGB = 241;    
    public Material buildingDefaultMaterial;
    public Material buildingMaterial;

    private float GetAverageInfection(List<PersonBase> personsPresent)
    {
        if (personsPresent == null || personsPresent.Count == 0) return 0;
        int cumulatedValue = 0;
        int counter = 0;
        foreach(PersonBase person in personsPresent)
        {            
            InfectionMgr infectionMgr = person.GetComponent<InfectionMgr>();
            if (infectionMgr.infected)
            {
                cumulatedValue += 1;
            }
            counter++;
        }
        float avg = (float)cumulatedValue / counter;

        return avg;
    }

    void UpdateColor()
    {        
        float avgInfection = GetAverageInfection(thisBuilding.GetPeopleForAverageInfection());

        int decrease = (int)(maxRGB * avgInfection);
        Color color = new Color32(maxRGB,      (byte)(int)Mathf.Clamp((maxRGB - decrease),0, maxRGB),         (byte)(int)Mathf.Clamp((maxRGB - decrease), 0, maxRGB),    255);
        buildingMaterial.color = color;
    }  

    void recursiveSetColorables(Transform obj)
    {
        Material material = null;
        if(obj.gameObject.GetComponent<Renderer>() != null) material = obj.gameObject.GetComponent<Renderer>().sharedMaterial;
        if (material != null && material == buildingDefaultMaterial)
        {
            obj.gameObject.GetComponent<Renderer>().sharedMaterial = buildingMaterial;
        }
        foreach(Transform child in obj)
        {
            recursiveSetColorables(child);
        }
    }


    private void Awake()
    {        
        thisBuilding = GetComponent<SupportsColoring>();
        buildingMaterial = Instantiate(Resources.Load("BuildingMat") as Material);
        recursiveSetColorables(transform);
    }


    WaitForSeconds waitForSeconds = new WaitForSeconds(2f);
    IEnumerator Start()
    {
        while (true)
        {
            UpdateColor();
            yield return waitForSeconds;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
