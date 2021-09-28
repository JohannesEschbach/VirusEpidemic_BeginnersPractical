using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{

    private Slider loadingbar;
    public Canvas loadingCanvas;

    private IEnumerator UpdateBar()
    {
        while (NetworkGraph.working == false) yield return 0;
        while(loadingbar.value < 1)
        {
            loadingbar.value = 1 - (NetworkGraph.routeQueue/*SinceSnapshot*/.Count / (float)Data.Instance.NumOfPeople());
            yield return 0;
        }
        loadingCanvas.gameObject.SetActive(false);
        Time.timeScale = 1;
        yield break;
    }


    // Start is called before the first frame update
    void Start()
    {
        loadingbar = GetComponent<Slider>();
        StartCoroutine(UpdateBar());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
