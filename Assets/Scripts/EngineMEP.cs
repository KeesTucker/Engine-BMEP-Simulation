using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineMEP : MonoBehaviour
{
    //Refrences to engine specs
    public TopEnd topEndSpecs;
    public BottomEnd bottomEndSpecs;
    public IntakeSystem intakeSystemSpecs;
    public ExhaustSystem exhaustSystemSpecs;
    private float maxPistonExtension;

    //Refrence to enviroment parameters (atmosphere etc)
    public EnviromentProperties enviromentProperties;

    //Realtime engine parameters
    [Range(0, 1)]
    public float throttle;
    [Range(450, 15000)]
    public float RPM;
    public float currentChamberPressure = 0;
    public float linearIntakeVelocity = 0;
    public float linearExhuastVelocity = 0;
    public float currentGasTemp = 0;

    //Static engine parameters
    public float fI; //Moody friction coefficent of the intake system
    public float fE; //Moody friction coefficent of the exhaust system

    //Variables used to calculate MEP
    float totalChamberPressure;
    int numIntakeSteps;
    public float MEP;

    //Refrence to graph drawer
    private GraphingController graphingController;

    //Time in between iterations
    private float timeStep;

    private void Start()
    {
        graphingController = FindObjectOfType<GraphingController>();

        maxPistonExtension = EngineHelperMethods.GetCrankRadius(bottomEndSpecs.stroke) + bottomEndSpecs.rodLength;
        currentChamberPressure = enviromentProperties.atmospherePressure;

        UpdateManifoldParams();

        UpdatePressure();
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
        enviromentProperties.UpdateAirProperties();

        //take this out once i have all strokes coded.
        currentChamberPressure = enviromentProperties.atmospherePressure;
        currentGasTemp = UnitHelperMethods.CelsiusToKelvin(enviromentProperties.airTemp);
        linearIntakeVelocity = 0;
        linearExhuastVelocity = 0;

        //Reset the MEP calculation each cycle
        totalChamberPressure = 0;
        numIntakeSteps = 0;

        //Update time step
        timeStep = (60f / RPM) / 720;

        //Iterate through an entire cycle. ATM each iteration is one degree, in game this will be based on framerate and RPM.
        for (float A = 0; A < 720; A++)
        {
            //Previous crank position update
            float previousA = UnitHelperMethods.DegreesToRadians(720);
            if (A != 0)
            {
                previousA = UnitHelperMethods.DegreesToRadians(A - 1);
            }

            //Get chamber gas parameters at this stage of the cycle
            (float, float, float, float, float) currentChamberFlow = EngineHelperMethods.GetPressureAtCrankPosition(throttle, 
                fI, fE, UnitHelperMethods.DegreesToRadians(A), 0,
                currentChamberPressure, enviromentProperties.atmospherePressure, enviromentProperties.atmosphereDensity,
                enviromentProperties.atmosphereViscosity, bottomEndSpecs, intakeSystemSpecs, exhaustSystemSpecs, currentGasTemp,
                RPM, previousA, timeStep, linearIntakeVelocity, linearExhuastVelocity, topEndSpecs, enviromentProperties.airDensityRatio, UnitHelperMethods.CelsiusToKelvin(enviromentProperties.airTemp));

            //Pull values out of the tuple returned by GetPressureAtCrankPosition(), 3rd value is for optional data collection
            currentChamberPressure = currentChamberFlow.Item1;
            linearIntakeVelocity = currentChamberFlow.Item2;
            linearExhuastVelocity = currentChamberFlow.Item3;
            currentGasTemp = currentChamberFlow.Item5;

            //Get current piston position to draw data/volume graph
            float currentPistonPosition = EngineHelperMethods.GetPistonPosition(
                EngineHelperMethods.GetCrankRadius(bottomEndSpecs.stroke), UnitHelperMethods.DegreesToRadians(A),
                bottomEndSpecs.rodLength, RPM);

            //Add data to data/volume graph
            graphingController.data1.Add(new Vector3(maxPistonExtension - currentPistonPosition, currentChamberPressure - enviromentProperties.atmospherePressure));
            graphingController.data2.Add(new Vector3(maxPistonExtension - currentPistonPosition, currentGasTemp, 0));
            graphingController.data3.Add(new Vector3(maxPistonExtension - currentPistonPosition, currentChamberFlow.Item4, 0));

            //Update MEP vars
            numIntakeSteps++;
            totalChamberPressure += currentChamberPressure;
        }
        //Get MEP (Mean Effective Pressure)
        MEP = totalChamberPressure / numIntakeSteps / 1000;

        graphingController.GraphPoints();
    }

    //Update manifold params relying on manifold quality etc (call if manifold is changed, or intake config is changed)
    private void UpdateManifoldParams()
    {
        float averageCrossSectionalAreaOfManifold = EngineHelperMethods.GetIntakeCrossSectionalArea(intakeSystemSpecs.intakeSystemSharedVolume + intakeSystemSpecs.intakeSystemIndividualVolume[0], intakeSystemSpecs.intakeSystemLength[0]);
        float aveDiameterOfManifold = UnitHelperMethods.AreaToRadius(averageCrossSectionalAreaOfManifold);
        fI = EngineHelperMethods.GetCoefficentOfFriction(aveDiameterOfManifold, intakeSystemSpecs.intakeSystemRoughness, intakeSystemSpecs.intakeSystemDesignQuality);
    }

    //Update exhaust params relying on exhaust quality etc (call if exhaust is changed, or exhaust config is changed)
    //Change all the zeroe indexes to something that takes into account the average for all the headers and exhausts not just the first one.
    private void UpdateExhaustParams()
    {
        float volumeOfExhaust = Mathf.PI * Mathf.Pow(exhaustSystemSpecs.exhaustPipeDiameter / 2, 2) * exhaustSystemSpecs.exhaustPipeLength[exhaustSystemSpecs.cylinderIndexToPipe[0]]
    + Mathf.PI * Mathf.Pow(exhaustSystemSpecs.headerDiameter / 2, 2) * exhaustSystemSpecs.headerLength[0];
        float averageCrossSectionalAreaOfExhaust = volumeOfExhaust / (exhaustSystemSpecs.headerLength[0] + exhaustSystemSpecs.exhaustPipeLength[exhaustSystemSpecs.cylinderIndexToPipe[0]]);
        float averageDiameterOfExhaust = UnitHelperMethods.AreaToRadius(averageCrossSectionalAreaOfExhaust) * 2;
        fE = EngineHelperMethods.GetCoefficentOfFriction(averageDiameterOfExhaust, exhaustSystemSpecs.exhaustSystemRoughness, exhaustSystemSpecs.exhaustSystemDesignQuality);
    }
}