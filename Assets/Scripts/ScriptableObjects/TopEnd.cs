using UnityEngine;

[CreateAssetMenu(fileName = "TopEnd", menuName = "Engine/Config/TopEnd", order = 1)]
public class TopEnd : ScriptableObject
{
    [Header("Number of degrees to adjust spark plug energization by.")]
    /// <summary>
    /// Number of degrees to adjust spark plug energization by.
    /// </summary>
    public float ignitionTiming = 0;
    
    public int numIntakeValves = 1;
    public int numExhaustValves = 1;

    [Header("Clockwise past TDC, Format: [Valve1Open, Valve2Close, ValveNOpen, ValveNClose]")]
    /// <summary>
    /// Clockwise past TDC
    /// Format: [Valve1Open, Valve2Close, ValveNOpen, ValveNClose]
    /// In degrees
    /// </summary>
    public float[] intakeValveTimings = { 710, 235 };

    [Header("Clockwise past TDC, Format: [Valve1Open, Valve2Close, ValveNOpen, ValveNClose]")]
    /// <summary>
    /// Clockwise past TDC
    /// Format: [Valve1Open, Valve2Close, ValveNOpen, ValveNClose]
    /// In degrees
    /// </summary>
    public float[] exhaustValveTimings = { 495, 20 };

    public float intakeValveDiameter = 0.0425f;
    public float exhaustValveDiameter = 0.0425f;
}