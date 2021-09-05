using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphingController : MonoBehaviour
{
    public LineRenderer[] lineRenderers;

    public List<Vector3> data1 = new List<Vector3>();
    public List<Vector3> data2 = new List<Vector3>();
    public List<Vector3> data3 = new List<Vector3>();

    private void Start()
    {
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].SetPositions(new Vector3[] { Vector3.zero });
        }
    }

    public void GraphPoints()
    {
        lineRenderers[0].positionCount = data1.Count;
        lineRenderers[0].SetPositions(data1.ToArray());
        lineRenderers[1].positionCount = data2.Count;
        lineRenderers[1].SetPositions(data2.ToArray());
        lineRenderers[2].positionCount = data3.Count;
        lineRenderers[2].SetPositions(data3.ToArray());
        data1 = new List<Vector3>();
        data2 = new List<Vector3>();
        data3 = new List<Vector3>();
    }
}
