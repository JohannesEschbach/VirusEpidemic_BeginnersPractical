using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    [HideInInspector]
    public string vertex = "none";
    private static int nextVertex = 0;    

    //[HideInInspector]
    //public Vector3 position;

    [HideInInspector]
    public int moduleID;

    public bool isPedestrian;
    public bool isBusstop;

    public string connecting = "";

    //Neighbourlist only read once during first initialisation
    public List<GameObject> neighbours = new List<GameObject>();

    private void Awake()
    {
        vertex = nextVertex.ToString();
        nextVertex++;
    }

    private void Start()
    {

    }
}