using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    public void togglePanel(bool on)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        group.alpha = System.Convert.ToInt32(on);
        group.interactable = on;
        group.blocksRaycasts = on;
    }
}
