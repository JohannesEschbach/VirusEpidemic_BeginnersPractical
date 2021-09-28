using System.Collections;
using System.Threading;
using UnityEngine;

public class CoroutineStarter : MonoBehaviour
{

    private Thread plotter;

    NetworkGraph networkGraph = NetworkGraph.Instance;


    public Vertex bugFixVertex;


    // Use this for initialization
    public void Start()
    {
        StartCoroutine(InfectionMgr.copyDictionaries());
        StartCoroutine(InfectionMgr.UpdateCoursesOfDisease());

        networkGraph.startThread();
    }
}