using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphingController : MonoBehaviour
{
    public LineRenderer[] lineRenderers;

    public List<Vector3> pressurePoints = new List<Vector3>();
    public List<Vector3> temperaturePoints = new List<Vector3>();
    public List<Vector3> frictionPoints = new List<Vector3>();

    private void Start()
    {
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].SetPositions(new Vector3[] { Vector3.zero });
        }
    }

    public void GraphPoints()
    {
        lineRenderers[0].positionCount = pressurePoints.Count;
        lineRenderers[0].SetPositions(pressurePoints.ToArray());
        lineRenderers[1].positionCount = temperaturePoints.Count;
        lineRenderers[1].SetPositions(temperaturePoints.ToArray());
        lineRenderers[2].positionCount = frictionPoints.Count;
        lineRenderers[2].SetPositions(frictionPoints.ToArray());
        pressurePoints.Clear();
        temperaturePoints.Clear();
        frictionPoints.Clear();
    }
}
