using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayTime
{
    //Base Speed: Seconds = Minutes; Minutes = Hours
    //Outside infection chance is base per second (due to movement speed). Inside per Minute. Gameplay vs Realism?

    public int day; 
    public int hour;
    public float minute;
    public static float scale = 0.65f; //NOTE: REDO ALL FLOAT BASED TIME COMPARISONS AS DAYTIME BASED ONES

    private static float timeAtStart = 0; //Saved Time.time at end of last game session //Already Scaled minutes at start

    public DayTime(int _hour, float _minute) //Used mostly for schedule arrangements
    {
        day = (int)((Time.timeSinceLevelLoad * scale + timeAtStart) / (60 * 24));
        hour = _hour;
        minute = _minute;
    }

    public DayTime(DayTime time)
    {
        day = time.day;
        hour = time.hour;
        minute = time.minute;
    }

    public DayTime()
    {
        day = (int)((Time.timeSinceLevelLoad * scale + timeAtStart) / (60 * 24));
        hour = (int)((Time.timeSinceLevelLoad * scale + timeAtStart) / 60) % 24;
        minute = (Time.timeSinceLevelLoad * scale + timeAtStart) % 60;
    }

    public static float scaledTimeToUnityTime(float mins)
    {
        return mins / scale;
    }

    public void AddUnscaledMinute(float mins)
    {
        AddMinute(mins * scale);
    }

    public void AddMinute(float mins)
    {
        day += (int)(mins / (60 * 24));
        hour += (int)(mins / 60) % 24;
        minute += mins % 60;
    }

    public void AddDay(int days)
    {
        day += days;

    }

    public float getMinutes()
    {
        return 24 * 60 * day + 60 * hour + minute;
    }

    public float getMinutesFromSessionStart()
    {
        float minSinceStart = getMinutes() - timeAtStart;
        if (minSinceStart > 0) return minSinceStart;
        else return 0;
    }

    //For in day-scheduling (Day is ignored)
    public bool after(DayTime b)
    {
        if (hour > b.hour) return true;
        else if (hour == b.hour)
        {
            return (minute >= b.minute);
        }
        else return false;
    }

    public static bool operator >(DayTime a, DayTime b) //THIS COMPARISON IS COMPUTATIONALLY HORRIBLE PLZ FIX
    {
        if (24 * 60 * a.day + 60 * a.hour + a.minute > 24 * 60 * b.day + 60 * b.hour + b.minute) return true;
        else return false;
    }

    public static bool operator <(DayTime a, DayTime b)
    {
        if (24 * 60 * a.day + 60 * a.hour + a.minute < 24 * 60 * b.day + 60 * b.hour + b.minute) return true;
        else return false;
    }

    public static float operator -(DayTime a, DayTime b)
    {
        return (24 * 60 * a.day + 60 * a.hour + a.minute) - (24 * 60 * b.day + 60 * b.hour + b.minute);
    }
}
