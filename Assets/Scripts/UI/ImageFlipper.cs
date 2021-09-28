using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageFlipper : MonoBehaviour
{
    public void Flip()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.Rotate(new Vector3(0,0,180));
    }

}
