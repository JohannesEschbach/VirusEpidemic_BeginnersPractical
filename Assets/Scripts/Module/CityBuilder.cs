using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBuilder : MonoBehaviour
{
    public Populator populator;
    private Data data = Data.Instance;


    public static int modulesPerRow = 3;

    public GameObject centerHospitalModule; //Later List to randomly choose from with multiple versions
    public GameObject centerModule;
    public GameObject suburbHospitalModule;
    public GameObject suburbModule;

    private static List<List<Module>> modules = new List<List<Module>>();


    private int moduleWidth = 300;

    private void SetUpModules()
    {
        centerHospitalModule.SetActive(false);
        for(int y = 0; y < modulesPerRow; y++)
        {
            modules.Add(new List<Module>());
            for(int x = 0; x < modulesPerRow; x++)
            {
                GameObject moduleObject;         
                Vector3 modulePos = new Vector3(moduleWidth * x, 0, moduleWidth * y);
                if (x + 1 == modulesPerRow / 2 && y + 1 == modulesPerRow / 2) moduleObject = Instantiate(centerHospitalModule, modulePos, Quaternion.identity);
                else if (x + 1 > 0.25 * modulesPerRow && x + 1 <= 0.75 * modulesPerRow && y + 1 > 0.25 * modulesPerRow && y + 1 <= 0.75 * modulesPerRow) moduleObject = Instantiate(centerModule, modulePos, Quaternion.identity);
                else moduleObject = Instantiate(suburbModule, modulePos, Quaternion.identity);

                moduleObject.transform.RotateAround(modulePos + new Vector3(moduleWidth / 2f, 0, moduleWidth / 2f), transform.up, 90 * Random.Range(0, 4));
                
                Module module = moduleObject.GetComponent<Module>();
                float edgeCostsMod = 2 * Mathf.Abs(x - modulesPerRow / 2) / (modulesPerRow / 2) * 2 * Mathf.Abs(y - modulesPerRow / 2) / (modulesPerRow / 2);
                module.edgeCostsModifier = edgeCostsMod;

                modules[y].Add(module);
                moduleObject.SetActive(true);

            }
        }
    }

    public static List<List<Module>> GetModules() { return modules; }

    public static Module GetModuleByIndex(int x, int y)
    {
        return modules[y][x];
    }

    public static void FocusModule(int x, int y)
    {
        Module module = GetModuleByIndex(x, y);
        Module.FocusModule(module);
    }

    private void Awake()
    {
        SetUpModules();
    }

    private void Start()
    {
        populator = GetComponent<Populator>();
        //Needs to be called after Module Awake() and BuildingBase Awake()
        populator.Populate(data.GetHomes(), data.GetOffices(), data.GetHospitals(), data.GetStores(), data.GetSocials());
    }

    //TESTSTUFF:
     //public GameObject personPrefab;
     //private int i = 0;

     /*private void Update()
     {
         if(i < 200)
         {
             for(int j=0; j < 70; j++)
             {
                 GameObject person = Instantiate(personPrefab);
                 person.SetActive(true);
                 i++;
             }
         }
     }*/

}