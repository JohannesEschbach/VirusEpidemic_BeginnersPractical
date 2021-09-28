using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    Text text;

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Increase()
    {        
        Time.timeScale += 1;
    }

    public void Decrease()
    {
        if (Time.timeScale == 0) return;
        if (Time.timeScale > 1) Time.timeScale -= 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0;
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        DayTime time = new DayTime(); //SUPER INEFFICIENT!!
        text.text = "Day: " + time.day + " Time: " + time.hour + ":" + (int)time.minute;
    }
}
