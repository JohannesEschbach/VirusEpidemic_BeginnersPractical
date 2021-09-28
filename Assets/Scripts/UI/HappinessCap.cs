using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HappinessCap : MonoBehaviour
{
    Slider slider;
    float prevValue;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    
    public void CapSlider()
    {
        if (Stats.happiness < 0)
        {
            slider.value = prevValue;
        }
        else prevValue = slider.value;
    }
}
