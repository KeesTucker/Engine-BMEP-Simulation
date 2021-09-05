using UnityEngine;

[CreateAssetMenu(fileName = "BottomEnd", menuName = "Engine/Config/BottomEnd", order = 1)]
public class BottomEnd : ScriptableObject
{
    /// <summary>
    /// Stroke in m
    /// </summary>
    public float stroke = 0.0634f;
    /// <summary>
    /// Bore in m
    /// </summary>
    public float bore = 0.095f;

    public float compressionRatio = 11.9f;

    /// <summary>
    /// Rod length in mm. Rod length must be longer than diameter of crank so must be longer than bore + margin.
    /// </summary>
    public float rodLength = 0.114f;
}