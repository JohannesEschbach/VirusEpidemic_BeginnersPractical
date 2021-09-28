using System.Collections.Generic;
using UnityEngine;

public class SleepHandlerQueue : MonoBehaviour
{
    private class TimeComparer : IComparer<System.Tuple<PersonBase, float>>
    {
        /// <summary>
        /// Comparer for <see cref="System.Tuple{PersonBase, float}"/> where float is WakeUpTime (Unity game time). Comparison is based on WakeUpTime.
        /// </summary>
        int IComparer<System.Tuple<PersonBase, float>>.Compare(System.Tuple<PersonBase, float> x, System.Tuple<PersonBase, float> y)
        {
            float timeX = x.Item2;
            float timeY = y.Item2;

            if (timeX < timeY) return 1;
            else if (timeX > timeY) return -1;
            else return 0;
        }
    }

    private static PriorityQueuePersonTime prio;
    public int queueCount;

    /// <summary>
    /// Sets the wake up time and pushes the tupel of person and wake up time to the priority queue.
    /// </summary>
    public static void SetWakeUpTime(DayTime time, PersonBase person)
    {
        float minutes = time.getMinutesFromSessionStart(); 
        System.Tuple<PersonBase, float> tuple = new System.Tuple<PersonBase, float>(person, minutes);
        prio.Push(tuple);
    }

    /// <summary>
    /// Sets the person active before its scheduled wake up time. Removes the respective tupel from the wake up priotrity queue.
    /// </summary>
    public static void WakeEarly(PersonBase person)
    {
        if (prio.Remove(person) != null)
        {
            person.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        IComparer<System.Tuple<PersonBase, float>> comparer = new TimeComparer();
        prio = new PriorityQueuePersonTime(comparer);
    }

    void Update()
    {
        queueCount = prio.Count;
        DayTime time = new DayTime();
        //float time = Time.timeSinceLevelLoad;
        while (prio.Count > 0 && prio.Top().Item2 < time.getMinutesFromSessionStart())
        {
            PersonBase person = prio.Pop().Item1;
            if(person.Alive) person.gameObject.SetActive(true);
        }    
    }
}
