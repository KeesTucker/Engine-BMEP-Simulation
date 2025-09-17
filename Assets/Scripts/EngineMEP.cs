using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineMEP : MonoBehaviour
{
    //References to engine specs
    public TopEnd topEndSpecs;
    public BottomEnd bottomEndSpecs;
    public IntakeSystem intakeSystemSpecs;
    public ExhaustSystem exhaustSystemSpecs;
    private float maxPistonExtension;

    //Reference to environment parameters (atmosphere etc)
    public EnvironmentProperties environmentProperties;

    //Realtime engine parameters
    [Range(0, 1)]
    public float throttle;
    [Range(450, 15000)]
    public float RPM;
    public float currentChamberPressure = 0;
    public float linearIntakeVelocity = 0;
    public float linearExhaustVelocity = 0;
    public float currentGasTemp = 0;

    //Static engine parameters
    public float intakeFrictionCoefficient; //Moody friction coefficient of the intake system
    public float exhaustFrictionCoefficient; //Moody friction coefficient of the exhaust system

    //Variables used to calculate MEP
    float totalChamberPressure;
    int numIntakeSteps;
    private int currentCrankStep;
    public float MEP;

    //Reference to graph drawer
    private GraphingController graphingController;

    //Time in between iterations
    private float timeStep;

    private void Start()
    {
        graphingController = FindObjectOfType<GraphingController>();

        maxPistonExtension = EngineHelperMethods.GetCrankRadius(bottomEndSpecs.stroke) + bottomEndSpecs.rodLength;
        currentChamberPressure = environmentProperties.atmospherePressure;
        currentGasTemp = UnitHelperMethods.CelsiusToKelvin(environmentProperties.airTemp);
        linearIntakeVelocity = 0f;
        linearExhaustVelocity = 0f;
        currentCrankStep = 0;

        UpdateManifoldParams();
        UpdateExhaustParams();
    }

    private void Update()
    {
        UpdateManifoldParams();
        UpdateExhaustParams();
        UpdatePressure();
    }

    private void UpdatePressure()
    {
        //Check for a change in atmosphere
        environmentProperties.UpdateAirProperties();

        if (currentCrankStep == 0)
        {
            totalChamberPressure = 0;
            numIntakeSteps = 0;
        }

        timeStep = (60f / RPM) / 720f;

        int previousStep = currentCrankStep == 0 ? 719 : currentCrankStep - 1;
        float previousAngle = UnitHelperMethods.DegreesToRadians(previousStep);
        currentCrankStep = (currentCrankStep + 1) % 720;
        float currentAngle = UnitHelperMethods.DegreesToRadians(currentCrankStep);

        (float chamberPressure, float intakeVelocity, float exhaustVelocity, float dataPoint, float gasTemp) currentChamberFlow =
            EngineHelperMethods.GetPressureAtCrankPosition(throttle, intakeFrictionCoefficient, exhaustFrictionCoefficient,
            currentAngle, 0, currentChamberPressure, environmentProperties.atmospherePressure,
            environmentProperties.atmosphereDensity, environmentProperties.atmosphereViscosity, bottomEndSpecs,
            intakeSystemSpecs, exhaustSystemSpecs, currentGasTemp, RPM, previousAngle, timeStep, linearIntakeVelocity,
            linearExhaustVelocity, topEndSpecs, environmentProperties.airDensityRatio,
            UnitHelperMethods.CelsiusToKelvin(environmentProperties.airTemp));

        currentChamberPressure = currentChamberFlow.chamberPressure;
        linearIntakeVelocity = currentChamberFlow.intakeVelocity;
        linearExhaustVelocity = currentChamberFlow.exhaustVelocity;
        currentGasTemp = currentChamberFlow.gasTemp;

        float currentPistonPosition = EngineHelperMethods.GetPistonPosition(
            EngineHelperMethods.GetCrankRadius(bottomEndSpecs.stroke), currentAngle, bottomEndSpecs.rodLength, RPM);

        graphingController.pressurePoints.Add(new Vector3(maxPistonExtension - currentPistonPosition,
            currentChamberPressure - environmentProperties.atmospherePressure));
        graphingController.temperaturePoints.Add(new Vector3(maxPistonExtension - currentPistonPosition, currentGasTemp, 0));
        graphingController.frictionPoints.Add(new Vector3(maxPistonExtension - currentPistonPosition, currentChamberFlow.dataPoint, 0));

        numIntakeSteps++;
        totalChamberPressure += currentChamberPressure;

        if (currentCrankStep == 0)
        {
            MEP = totalChamberPressure / numIntakeSteps / 1000f;
            graphingController.GraphPoints();
        }
    }

    //Update manifold params relying on manifold quality etc (call if manifold is changed, or intake config is changed)
    private void UpdateManifoldParams()
    {
        float averageCrossSectionalAreaOfManifold = EngineHelperMethods.GetIntakeCrossSectionalArea(intakeSystemSpecs.intakeSystemSharedVolume + intakeSystemSpecs.intakeSystemIndividualVolume[0], intakeSystemSpecs.intakeSystemLength[0]);
        float aveDiameterOfManifold = UnitHelperMethods.AreaToRadius(averageCrossSectionalAreaOfManifold);
        intakeFrictionCoefficient = EngineHelperMethods.GetCoefficientOfFriction(aveDiameterOfManifold, intakeSystemSpecs.intakeSystemRoughness, intakeSystemSpecs.intakeSystemDesignQuality);
    }

    //Update exhaust params relying on exhaust quality etc (call if exhaust is changed, or exhaust config is changed)
    private void UpdateExhaustParams()
    {
        float totalHeaderLength = 0f;
        foreach (float length in exhaustSystemSpecs.headerLength)
        {
            totalHeaderLength += length;
        }

        float totalPipeLength = 0f;
        foreach (float length in exhaustSystemSpecs.exhaustPipeLength)
        {
            totalPipeLength += length;
        }

        float headerVolume = Mathf.PI * Mathf.Pow(exhaustSystemSpecs.headerDiameter / 2f, 2f) * totalHeaderLength;
        float pipeVolume = Mathf.PI * Mathf.Pow(exhaustSystemSpecs.exhaustPipeDiameter / 2f, 2f) * totalPipeLength;
        float totalVolume = headerVolume + pipeVolume;
        float totalLength = totalHeaderLength + totalPipeLength;
        float averageCrossSectionalAreaOfExhaust = totalVolume / totalLength;
        float averageDiameterOfExhaust = UnitHelperMethods.AreaToRadius(averageCrossSectionalAreaOfExhaust) * 2f;
        exhaustFrictionCoefficient = EngineHelperMethods.GetCoefficientOfFriction(averageDiameterOfExhaust, exhaustSystemSpecs.exhaustSystemRoughness, exhaustSystemSpecs.exhaustSystemDesignQuality);
    }
}