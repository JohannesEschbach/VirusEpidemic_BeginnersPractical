using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestrictionMgr : MonoBehaviour
{
    private static RestrictionMgr instance;
    private RestrictionMgr() { }
    public static RestrictionMgr Instance
    {
        get
        {
            return instance;
        }
    }

    //Budget Effects
    public float hourlyBudgetRecovery = 0.005f;
    public List<float> maxHourlyCostContributions;
    public List<float> currentHourlyCostContributions;

    private float NormalizeCosts(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }

    //Happiness Effects
    public List<float> maxHappinessContribution;

    public List<float> currentHappinessContribution;//Same order as above    

    public float maxHappinessPenalty = 0.5f; //Between 1 and 0 depending on % infected
   

    private  float NormalizeHappiness(float min, float max, float value)
    {
        return -maxHappinessPenalty + (value - min) / (max - min) * (1 + maxHappinessPenalty);
    }


    //Restrictions
    public int maskRules = 0; //0: No masks, 1: Masks inside (except home & social), 2: Mask inside and outside

    public void setMaskRules(float _level)
    {
        int level = (int)_level;
        currentHappinessContribution[1] = (1 - NormalizeHappiness(0, 2, level) - maxHappinessPenalty) * maxHappinessContribution[1];
        currentHourlyCostContributions[1] = NormalizeCosts(0, 2, level) * maxHourlyCostContributions[1];
        maskRules = level;
        updateHappiness();
    }

    private  float maxDistance = 2f;
    private  float minDistance = 0.5f;
    public  float minimumDistance = 0.5f; //Outside, Offices and Store

    public  void setMinimumDistance(float distance)
    {
        currentHappinessContribution[5] = (1 - NormalizeHappiness(minDistance, maxDistance, distance) - maxHappinessPenalty)  * maxHappinessContribution[5];
        currentHourlyCostContributions[5] = NormalizeCosts(minDistance, maxDistance, distance) * maxHourlyCostContributions[5];
        minimumDistance = distance;
        updateHappiness();

    }

    private  bool GastroGlobalClosed = false;

    public  bool isGastroClosed()
    {
        return GastroGlobalClosed;
    }

    public  int maxMeetingSizeRestriction = 10;
    public  int MeetingSize { get; private set; } = int.MaxValue;

    public  void setMeetingSize(float _size)
    {
        int size = (int)_size;
        if (size < maxMeetingSizeRestriction)
        {
            currentHappinessContribution[0] = NormalizeHappiness(0, maxMeetingSizeRestriction, size - 1) * maxHappinessContribution[0];
            currentHourlyCostContributions[0] = (1 - NormalizeCosts(0, maxMeetingSizeRestriction, size - 1)) * maxHourlyCostContributions[0];
            MeetingSize = size;
        }
        else
        {
            currentHappinessContribution[0] = maxHappinessContribution[0];
            currentHourlyCostContributions[0] = 0f;
            MeetingSize = int.MaxValue;

        }

        updateHappiness();
    }

    public  int maxClientPerStoreRestriction = 10;
    public int minClientPerStoreRestriction = 4;
    public  int ClientsPerStore { get; set; } = int.MaxValue;

    public  void setClientPerStore(float _size)
    {
        int size = (int)_size;
        if (size < maxClientPerStoreRestriction)
        {
            currentHappinessContribution[3] = NormalizeHappiness(minClientPerStoreRestriction, maxClientPerStoreRestriction, size - 1) * maxHappinessContribution[3];
            currentHourlyCostContributions[3] = (1 - NormalizeCosts(minClientPerStoreRestriction, maxClientPerStoreRestriction, size - 1)) * maxHourlyCostContributions[3];

            MeetingSize = size;
        }
        else
        {
            currentHappinessContribution[3] = maxHappinessContribution[3];
            currentHourlyCostContributions[3] = 0f;
            MeetingSize = int.MaxValue;

        }
        updateHappiness();
    }

    public  int ClientsPerSocial { get; set; } = int.MaxValue;
    public  int maxClientPerSocialRestriction = 20;

    public  void setClientPerSocial(float _size)
    {
        int size = (int)_size;
        if (size < maxClientPerSocialRestriction)
        {                        
            currentHappinessContribution[4] = NormalizeHappiness(0, maxClientPerSocialRestriction, size) * maxHappinessContribution[4];
            currentHourlyCostContributions[4] = (1 - NormalizeCosts(0, maxClientPerSocialRestriction, size)) * maxHourlyCostContributions[4];

            MeetingSize = size;            
        }
        else
        {
            currentHappinessContribution[4] = maxHappinessContribution[4];
            currentHourlyCostContributions[4] = 0f;
            MeetingSize = int.MaxValue;

        }
        GastroGlobalClosed = (size == 0);
        updateHappiness();
    }

    //public  bool HomeOfficeGlobal = false;    
    public int numModulesInHomeOffice;

    public void toggleHomeOfficeGlobal(float v)
    {
        /*HomeOfficeGlobal = !HomeOfficeGlobal;
        if (HomeOfficeGlobal)
        {
            currentHappinessContribution[2] = -maxHappinessPenalty * maxHappinessContribution[2];
            currentHourlyCostContributions[2] = maxHourlyCostContributions[2];
        }
        else
        {
            currentHappinessContribution[2] = maxHappinessContribution[2];
            currentHourlyCostContributions[2] = 0;
        }
        Stats.updateHappiness(currentHappinessContribution);*/

        Data data = Data.Instance;

        bool value = System.Convert.ToBoolean((int)v);
        foreach (List<Module> row in CityBuilder.GetModules())
        {
            foreach (Module module in row)
            {
                data.setModuleHomeOfficeMode(module, value);
            }
        }
        /*if (value)
        {
            //currentHappinessContribution[2] = -maxHappinessPenalty * maxHappinessContribution[2];
            //currentHourlyCostContributions[2] = maxHourlyCostContributions[2];
            numModulesInHomeOffice = CityBuilder.modulesPerRow * CityBuilder.modulesPerRow;
        }
        else
        {
            //currentHappinessContribution[2] = maxHappinessContribution[2];
            //currentHourlyCostContributions[2] = 0;
            numModulesInHomeOffice = 0;
        }*/
        homeOfficeUpdateHappiness();

    }


    public void homeOfficeUpdateHappiness()
    {
        Data data = Data.Instance;

        numModulesInHomeOffice = 0;
        foreach (List<Module> row in CityBuilder.GetModules())
        {
            foreach (Module module in row)
            {
                if (data.isModuleHomeOfficeMode(module)) numModulesInHomeOffice++;
            }
        }

        int numOfModules = CityBuilder.modulesPerRow * CityBuilder.modulesPerRow;

        //currentHappinessContribution[2] = -maxHappinessPenalty * maxHappinessContribution[2] * numModulesInHomeOffice / (numOfModules);
        //currentHourlyCostContributions[2] = -maxHappinessContribution[2] * numModulesInHomeOffice / (numOfModules);

        currentHappinessContribution[2] = (1 - NormalizeHappiness(0, numOfModules, numModulesInHomeOffice) - maxHappinessPenalty) * maxHappinessContribution[2];
        currentHourlyCostContributions[2] =  NormalizeCosts(0, numOfModules, numModulesInHomeOffice) * maxHourlyCostContributions[2];

        updateHappiness();
    }

    public  void toggleLocalHomeOffice(float value)
    {
        Module focusedModule = Module.focusedModule;
        if (focusedModule == null)
        {
            restrictionSliders[6].value = 0;
            return;
        }
        Data.Instance.setModuleHomeOfficeMode(focusedModule, System.Convert.ToBoolean((int)value));
        homeOfficeUpdateHappiness();
    }

    public  bool isHomeOfficeMode(PersonBase person)
    {
        Data data = Data.Instance;
        BuildingBase office = data.GetWorkplace(person);        
        if (!((IsWorkplace)office).SupportsHomeOffice()) return false;
        if (office is BuildingSocial) return GastroGlobalClosed;
        
        Module module = data.buildingIsInModule(office);
        return data.isModuleHomeOfficeMode(module);
    }


    public IEnumerator UpdateBudgetHourly()
    {
        while (true)
        {
            Stats.updateBudget(currentHourlyCostContributions, hourlyBudgetRecovery);
            yield return new WaitForSeconds(DayTime.scaledTimeToUnityTime(60f));
        }
    }

    public List<Slider> restrictionSliders = new List<Slider>(); //In order of UI

    public void updateHappiness()
    {
        //Could be negative when more people get healthy and hence increase the penalty
        if (!Stats.updateHappiness(currentHappinessContribution))
        {
            restrictionSliders[0].value = 0;
            restrictionSliders[1].value = 0;
            restrictionSliders[2].value = 10;
            restrictionSliders[3].value = 10;
            restrictionSliders[4].value = 20;
            restrictionSliders[5].value = 0;
        }

    }


    private void Start()
    {
        instance = this;
        maxHappinessContribution = new List<float> {
            0.25f,    //meeting size            
            0.15f,    //Masks
            0.125f,  //HomeOffice
            0.075f,    //ClientsStore
            0.30f,    //ClientsSocial
            0.10f,   //MinimumDistance
        };
        currentHappinessContribution = new List<float>(maxHappinessContribution);
        maxHourlyCostContributions = new List<float>
        {
            0f,    //meeting size            
            0f,    //Masks
            0.005f,  //HomeOffice
            0.005f,    //ClientsStore
            0.005f,    //ClientsSocial
            0f,   //MinimumDistance
        };
        currentHourlyCostContributions = new List<float>() { 0, 0, 0, 0, 0, 0 };

        StartCoroutine(UpdateBudgetHourly());
    }

    private void Update()
    {
        maxHappinessPenalty = 1 - 2 * Stats.confirmedCurrentInfected / Stats.currentPopulation(); //no penalty when half the population infected
    }
}