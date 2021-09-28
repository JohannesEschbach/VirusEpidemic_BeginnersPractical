using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleColorer : MonoBehaviour
{
    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        ColorBlock colors = toggle.colors;
        colors.normalColor = isOn ? new Color(0.5f, 0.5f, 0.5f) : new Color(1, 1, 1);
        colors.selectedColor = isOn ? new Color(0.5f, 0.5f, 0.5f) : new Color(1, 1, 1);
        toggle.colors = colors;
    }
}
