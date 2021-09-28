using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopPanelToggle : MonoBehaviour
{
    
    private ToggleGroup toggleGroup;
    public GameObject canvasToShow;
    public GameObject canvasToHide;
    public Camera mainCam;
    public Camera UICam;
    public ToggleGroup moduleToggles;

    public void OnToggleValueChanged()
    {
        if (toggleGroup.AnyTogglesOn())
        {
            moduleToggles.SetAllTogglesOff();
        }
        canvasToShow.GetComponent<TogglePanel>().togglePanel(toggleGroup.AnyTogglesOn());
        canvasToHide.GetComponent<TogglePanel>().togglePanel(!toggleGroup.AnyTogglesOn());

        mainCam.gameObject.SetActive(!toggleGroup.AnyTogglesOn());
        UICam.gameObject.SetActive(toggleGroup.AnyTogglesOn());

    }

    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        OnToggleValueChanged();        
    }
}
