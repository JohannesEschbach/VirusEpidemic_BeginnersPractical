using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidePanel : MonoBehaviour
{
    private bool isDown;
    private RectTransform rectTransform; //getting reference to this component 

    float timeOfTravel; //time after object reach a target place 
    float currentTime; // actual floting time 
    float normalizedValue;
    private Vector3 startPosition;
    public Vector3 endPosition;
    private Vector3 start;
    private Vector3 end;

    IEnumerator LerpObject()
    {              
        while (currentTime <= timeOfTravel)
        {
            currentTime += Time.deltaTime;
            normalizedValue = currentTime / timeOfTravel; // we normalize our time 

            rectTransform.anchoredPosition = Vector3.Lerp(start, end, normalizedValue);
            yield return null;
        }
        isDown = !isDown;
    }

    public void toggleDrop()
    {
        timeOfTravel = 1;
        currentTime = 0;
        if (!isDown)
        {
            start = startPosition;
            end = endPosition;
        }
        else
        {
            start = endPosition;// startPosition = new Vector3(-175, -175, 0);
            end = startPosition;// endPosition = new Vector3(-175, 175, 0);
        }
        IEnumerator cachedCoroutine = LerpObject();
        StartCoroutine(cachedCoroutine);
    }

    private void Start()
    {        
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition3D;
    }
}
