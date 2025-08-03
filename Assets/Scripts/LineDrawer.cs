using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private float spacing = 1f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float width = 0.5f;
    private float baseScale = 10f;
    private List<GameObject> gos;

    private void Awake()
    {
        gos = new List<GameObject>();        
    }
    public void DrawGrid(int sideLength)
    {
        for(int i = gos.Count - 1;i >= 0;i--)
        {
            Destroy(gos[i]);
            gos.RemoveAt(i);
        }
        float scale = baseScale / sideLength;
        for (int i = 0; i <= sideLength; i++)
        {
            gos.Add(CreateLine(
                new Vector3(i * spacing * scale, 0f, 0f),
                new Vector3(i * spacing * scale, 0f, sideLength * spacing * scale),
                gameObject.transform));

            gos.Add(CreateLine(
                new Vector3(0f, 0f, i * spacing * scale),
                new Vector3(sideLength * spacing * scale, 0f, i * spacing * scale),
                gameObject.transform));
        }
    }
    private GameObject CreateLine(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject lrGO = new GameObject("Line");
        lrGO.transform.SetParent(parent, false);
        LineRenderer lr = lrGO.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.useWorldSpace = false;
        lr.startWidth = lr.endWidth = width;
        lr.SetPositions(new[] { start, end });
        return lrGO;
    }
}
