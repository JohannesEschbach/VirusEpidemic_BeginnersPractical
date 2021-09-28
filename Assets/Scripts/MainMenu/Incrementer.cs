using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Incrementer : MonoBehaviour
{
    Text text;
    public float value;
    public float lower;
    public float upper;
    public float stepSize = 1;

    public void Increment()
    {
        value = Mathf.Min(upper, value + stepSize);
        text.text = value.ToString();
    }

    public void Decrement()
    {
        value = Mathf.Max(lower, value - stepSize);
        text.text = value.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        text.text = value.ToString();

    }
}
