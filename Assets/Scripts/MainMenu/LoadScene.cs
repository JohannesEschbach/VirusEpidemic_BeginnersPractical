using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadScene : MonoBehaviour
{
    public VariableSetter varSetter;

    void Start()
    {

    }

    public void LoadGame()
    {
        varSetter.onPlay();
        SceneManager.LoadScene(1);
    }

}
