using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityModulesPanel : MonoBehaviour
{
    public GameObject moduleButtonPrefab;



    public void setSquares()
    {
        RectTransform rt = GetComponent<RectTransform>();
        float width = rt.rect.width;        

        int n = CityBuilder.modulesPerRow;

        float buttonWidth = width / n;

        for(int i = 0; i < n; i++)
        {
            for(int j = 0; j < n; j++)
            {
                GameObject buttonObj = Instantiate(moduleButtonPrefab, this.transform);
                RectTransform buttonRt = buttonObj.GetComponent<RectTransform>();

                buttonRt.anchoredPosition = new Vector3(j * buttonWidth + buttonWidth / 2, i * buttonWidth + buttonWidth / 2, 0);
                buttonRt.sizeDelta = new Vector2(buttonWidth, buttonWidth);

                ModuleButton buttonScript = buttonObj.GetComponent<ModuleButton>();
                buttonScript.x = j;
                buttonScript.y = i;
            }
        }
    }


    public void Start()
    {
        setSquares();
    }

    public void Update()
    {
        
    }
}

