using UnityEngine;

[CreateAssetMenu(fileName = "ExhaustSystem", menuName = "Engine/Config/ExhaustSystem", order = 1)]
public class ExhaustSystem : ScriptableObject
{
    /// <summary>
    /// Enum to keep track of available mufflers.
    /// </summary>
    public enum mufflerType { none };

    /// <summary>
    /// Length of the path the air must travel through the exhaust pipe.
    /// </summary>
    public float[] exhaustPipeLength = { 1.5f };

    /// <summary>
    /// Maps cylinder index to index of exhaust pipe. Eg. pipeIndex = cylinderIndexToPipe[cylinderIndex].
    /// </summary>
    public int[] cylinderIndexToPipe = { 0 };

    /// <summary>
    /// Diameter of the exhaust pipe. 
    /// </summary>
    public float exhaustPipeDiameter = 0.05f;

    /// <summary>
    /// The type of muffler used on this exaust.
    /// </summary>
    public mufflerType muffler = mufflerType.none;

    /// <summary>
    /// Volume of each cyclinder's individual piping (not piping but each cylinder's part of the manifold.
    /// </summary>
    public float[] headerLength = { 0.2f };

    /// <summary>
    /// Volume of the shared part of the manifold for all cylinders.
    /// </summary>
    public float headerDiameter = 0.03f;

    /// <summary>
    /// Airflow design quality correction, between 0 and 1. Higher is better.
    /// </summary>
    public float exhaustSystemDesignQuality = 0.5f;

    /// <summary>
    /// Empirical material roughness, somewhere in between 0.00004 & 0.0002,
    /// Steel is 0.00015
    /// </summary>
    public float exhaustSystemRoughness = 0.0007f;
}