using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGridRenderer : Graphic
{
    public Vector2Int gridSize = new Vector2Int(1, 1);
    public float thickness = 10f;

    public int scaleX;
    public int scaleY;

    public List<Text> yLabels;
    public List<Text> xLabels;
    public List<UILineRenderer> uiLineRenderers;

    float width;
    float height;
    float cellWidth;
    float cellHeight;

    public void DoubleGridX()
    {
        scaleX *= 2;
        UpdateLabelsX();        
        foreach(UILineRenderer line in uiLineRenderers)
        {
            line.DoubleScaleLineX();
        }
    }

    public void DoubleGridY()
    {
        scaleY *= 2;
        UpdateLabelsY();
        foreach (UILineRenderer line in uiLineRenderers)
        {
            line.DoubleScaleLineY();
        }
    }

    void UpdateLabelsX()
    {
        int i = 1;
        foreach (Text label in xLabels)
        {
            int hours = scaleX * i;
            if (hours % 24 == 0)
            {
                label.text = "Day " + ((scaleX * i) / 24).ToString();
            }
            else
            {
                label.text = "";
            }
            i++;
        }
    }


    void UpdateLabelsY()
    {
        int i = 1;
        foreach (Text label in yLabels)
        {
            label.text = (scaleY * i).ToString();
            i++;
        }
    }

    protected override void Start()
    {
        xLabels = new List<Text>();
        foreach(Transform labelObj in this.gameObject.transform.GetChild(0).transform){
            xLabels.Add(labelObj.GetComponent<Text>());
        }
        
        yLabels = new List<Text>();
        foreach (Transform labelObj in this.gameObject.transform.GetChild(1).transform){
            yLabels.Add(labelObj.GetComponent<Text>());
        }

        scaleX = 24 / gridSize.x;
        scaleY = 25;

        UpdateLabelsY();
        UpdateLabelsX();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        cellWidth = width / (float)gridSize.x;
        cellHeight = height / (float)gridSize.y;

        int count = 0;
        for(int y = 0; y < gridSize.y; y++)
        {
            for(int x = 0; x < gridSize.x; x++)
            {
                DrawCell(x, y, count, vh);
                count++;
            }
        }
    }

    private void DrawCell(int x, int y, int index, VertexHelper vh)
    {
        float xPos = cellWidth * x;
        float yPos = cellHeight * y;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos);
        vh.AddVert(vertex);

        //vh.AddTriangle(0, 1, 2);
        //vh.AddTriangle(2, 3, 0);

        float widthSqr = thickness * thickness;
        float distanceSqr = widthSqr / 2f;
        float distance = Mathf.Sqrt(distanceSqr);

        vertex.position = new Vector3(xPos + distance, yPos + distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + (cellHeight - distance));
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + (cellWidth - distance), yPos + (cellHeight - distance));
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + (cellWidth - distance), yPos + distance);
        vh.AddVert(vertex);

        int offset = index * 8;

        vh.AddTriangle(offset + 0, offset + 1, offset + 5);
        vh.AddTriangle(offset + 5, offset + 4, offset + 0);


        vh.AddTriangle(offset + 1, offset + 2, offset + 6);
        vh.AddTriangle(offset + 6, offset + 5, offset + 1);

        vh.AddTriangle(offset + 2, offset + 3, offset + 7);
        vh.AddTriangle(offset + 7, offset + 6, offset + 2);


        vh.AddTriangle(offset + 3, offset + 0, offset + 4);
        vh.AddTriangle(offset + 4, offset + 7, offset + 3);
    }

}
