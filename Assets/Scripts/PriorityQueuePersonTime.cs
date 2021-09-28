using System;
using System.Collections.Generic;

public class PriorityQueuePersonTime : PriorityQueue<System.Tuple<PersonBase, float>>
{

    public PriorityQueuePersonTime(IComparer<System.Tuple<PersonBase, float>> comparer) : base(comparer) { }

    public System.Tuple<PersonBase, float> Remove(PersonBase person)
    {                       
        for(int i = 0; i < Count; i++)
        {
            System.Tuple<PersonBase, float> tuple = heap[i];
            if (tuple.Item1 == person)
            {
                heap[i] = heap[--Count];
                if (Count > 0) SiftDown(i);
                return tuple;
            }
        }
        return null;
    }
}