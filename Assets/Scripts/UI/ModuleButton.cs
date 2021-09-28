using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleButton : MonoBehaviour
{
    public int x;
    public int y;

    public void onPressed()
    {
        Camera.main.GetComponent<RTSCamera>().placeCamera(x, y);
        CityBuilder.FocusModule(x, y);
    }


}
