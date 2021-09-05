using UnityEngine;

[CreateAssetMenu(fileName = "IntakeSystem", menuName = "Engine/Config/IntakeSystem", order = 1)]
public class IntakeSystem : ScriptableObject
{
    /// <summary>
    /// Length of the path the air must travel through for each cylinder in meters
    /// </summary>
    public float[] intakeSystemLength = { 0.15f };

    /// <summary>
    /// Volume of each cyclinder's individual piping (not piping but each cylinder's part of the manifold.
    /// </summary>
    public float[] intakeSystemIndividualVolume = { 0.0005f };

    /// <summary>
    /// Volume of the shared part of the manifold for all cylinders.
    /// </summary>
    public float intakeSystemSharedVolume = 0.00225f;

    /// <summary>
    /// Airflow design quality correction, between 0 and 1. Higher is better.
    /// </summary>
    public float intakeSystemDesignQuality = 0.5f;

    /// <summary>
    /// Empirical material roughness, somewhere in between 0.00004 & 0.0002,
    /// Steel is 0.00015
    /// </summary>
    public float intakeSystemRoughness = 0.00012f;
}