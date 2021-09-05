using UnityEngine;
using System.Collections.Generic;

public static class EngineHelperMethods
{
    /// <summary>
    /// Gets the cylinder clearance volume
    /// </summary>
    /// <param name="displacementVolume">The CC rating</param>
    /// <param name="compressionRatio">Engine compression ratio</param>
    /// <returns>Returns clearance volume at top of cylinder</returns>
    public static float GetCylinderClearanceVolume(float displacementVolume, float compressionRatio)
    {
        float Vd = displacementVolume;
        float CR = compressionRatio;

        float Vc;
        Vc = Vd / (CR - 1);

        return Vc;
    }

    /// <summary>
    /// Gets the total cylinder volume
    /// </summary>
    /// <param name="displacementVolume">The CC rating</param>
    /// <param name="compressionRatio">Engine compression ratio</param>
    /// <returns>Returns volume of entire cylinder</returns>
    public static float GetTotalCylinderVolume(float displacementVolume, float compressionRatio)
    {
        float Vd = displacementVolume;
        float Vc = GetCylinderClearanceVolume(Vd, compressionRatio);
        float Vt;
        Vt = Vd + Vc;
        return Vt;
    }

    /// <summary>
    /// Convert RPM to angular velocity
    /// </summary>
    /// <param name="RPM">Revolutions per minute</param>
    /// <returns>Returns angular velocity of crank in Radians</returns>
    public static float GetAngularVelocityAtCrank(float RPM)
    {
        float w;
        w = 2 * Mathf.PI * RPM / 60f;
        return w;
    }

    /// <summary>
    /// Gets radius of crank
    /// </summary>
    /// <param name="stroke">Stroke of engine</param>
    /// <returns>Returns radius of crank</returns>
    public static float GetCrankRadius(float stroke)
    {
        float r;
        r = stroke / 2f;
        return r;
    }

    /// <summary>
    /// Gets velocity of piston at any point of the crank cycle
    /// </summary>
    /// <param name="crankRadius">Use GetCrankRadius(), in meters</param>
    /// <param name="crankAngle">Angle of crank at this point in time in radians</param>
    /// <param name="rodLength">Length of connecting rod from crank to piston, in meters</param>
    /// <returns>Linear velocity of piston, in m/s</returns>
    public static float GetPistonVelocity(float crankRadius, float crankAngle, float rodLength, float RPM)
    {
        float r = crankRadius;
        float A = crankAngle;
        float l = rodLength;

        float v;
        v = -r * Mathf.Sin(A) - ((r * r * Mathf.Sin(A) * Mathf.Cos(A)) / (Mathf.Sqrt(l * l - (r * r * Mathf.Sin(A) * Mathf.Sin(A)))));
        float w = GetAngularVelocityAtCrank(RPM);
        return v * w;
    }

    public static float GetPistonPosition(float crankRadius, float crankAngle, float rodLength, float RPM)
    {
        float r = crankRadius;
        float A = crankAngle;
        float l = rodLength;

        float x;
        x = r * Mathf.Cos(A) + Mathf.Sqrt(l * l - r * r * Mathf.Sin(A) * Mathf.Sin(A));
        return x;
    }

    /// <summary>
    /// Get angle of crank after a time step
    /// </summary>
    /// <param name="RPM">Current RPM</param>
    /// <param name="initialCrankPosition">Current position of crank in radians</param>
    /// <param name="time">Timestep</param>
    /// <returns>Returns angle of crank in radians</returns>
    public static float GetCrankPositionAfterSecs(float RPM, float initialCrankPosition, float time)
    {
        float T = time;
        float A = initialCrankPosition;

        float w = GetAngularVelocityAtCrank(RPM);
        float deltaA = w * T;

        A += deltaA;

        return A;
    }

    /// <summary>
    /// Gets the rate at which ignition chamber volume is changing with time.
    /// </summary>
    /// <param name="crankRadius">Use GetCrankRadius(), in meters</param>
    /// <param name="crankAngle">Angle of crank at this point in time in radians</param>
    /// <param name="rodLength">Length of connecting rod from crank to piston, in meters</param>
    /// <param name="bore">Cylinder diameter, in meters</param>
    /// <returns>Returns volume velocity in m^3/s</returns>
    public static float GetChamberVolumeVelocity(float crankRadius, float crankAngle, float rodLength, float bore, float RPM)
    {
        float vl = GetPistonVelocity(crankRadius, crankAngle, rodLength, RPM);
        float A = Mathf.PI * (bore / 2) * (bore / 2);

        float vV = vl * A;

        return vV;
    }

    /// <summary>
    /// Gets average velocity of gas moving through the intake system, negative is into cylinder, positive is out.
    /// </summary>
    /// <param name="crankRadius">Use GetCrankRadius(), in meters</param>
    /// <param name="crankAngle">Angle of crank at this point in time in radians</param>
    /// <param name="rodLength">Length of connecting rod from crank to piston, in meters</param>
    /// <param name="bore">Cylinder diameter, in meters</param>
    /// <param name="averageCrossSectionalAreaOfIntakeSystem">Average cross sectional area of intake manifold etc in m^2</param>
    /// <returns>Returns linear velocity of gas in m/s</returns>
    public static float GetIntakeVelocity(float RPM, float crankRadius, float crankAngle, float rodLength, float bore, float averageCrossSectionalAreaOfIntakeSystem)
    {
        float vl;
        vl = FluidDynamicsHelperMethods.GetAverageLinearVelocityOfFluidThroughOrifice(
            GetChamberVolumeVelocity(crankRadius, crankAngle, rodLength, bore, RPM), 
            averageCrossSectionalAreaOfIntakeSystem);
        return vl;
    }

    public static float GetIntakeCrossSectionalArea(float volume, float length)
    {
        float A = volume / length;
        return A;
    }

    /// <summary>
    /// Gets coefficent of friction of the intake system
    /// </summary>
    /// <param name="diameter">Average diameter of the system in meters, calculate using volume & length</param>
    /// <param name="roughness">Empirical material roughness, somewhere in between 0.00004 & 0.0002,
    /// Steel is 0.00015</param>
    /// <param name="intakeSystemQuality">A value from 0 to 1 dictating intake system design quality, higher is better</param>
    /// <returns></returns>
    public static float GetCoefficentOfFriction(float diameter, float roughness, float intakeSystemQuality)
    {
        float f;
        float qualityCorrection = ((0.5f - intakeSystemQuality) / 2f + 1f); //Account for the airflow design of the system, also offsets by 1 so f is at minimum 2x that of a straight pipe since this is a manifold it will have many turns and have higher friction than a striaght pipe
        f = Mathf.Pow(1.14f + 2f * Mathf.Log10(diameter / roughness), -2f) * qualityCorrection;
        return f;
    }

    /// <summary>
    /// Get current stroke from crank angle
    /// </summary>
    /// <param name="timing">A timing object with engine timing config</param>
    /// <param name="crankPosition">Crank position in degrees</param>
    /// <returns>Current strokes as a list of tuples containing an int identifier and a string name</returns>
    public static (int, string)[] GetCurrentStrokes(TopEnd timing, float crankPosition)
    {
        float A = crankPosition;

        List<(int, string)> currentStrokes = new List<(int, string)>();

        //NEED TO FIGURE SOMETHING OUT FOR MULTI VALVE HEADS

        if (A > timing.intakeValveTimings[0] || A <= timing.intakeValveTimings[1])
        {
            currentStrokes.Add((1, "Intake Stroke"));
        }
        if (A > timing.intakeValveTimings[1] && A <= 360 + timing.ignitionTiming)
        {
            currentStrokes.Add((2, "Compression Stroke"));
        }
        if (A > 360 + timing.ignitionTiming && A <= timing.exhaustValveTimings[0])
        {
            currentStrokes.Add((3, "Combustion Stroke"));
        }
        if (A > timing.exhaustValveTimings[0] || A <= timing.exhaustValveTimings[1])
        {
            currentStrokes.Add((4, "Exhaust Stroke"));
        }
        return currentStrokes.ToArray();
    }

    public static (float, float) GetPressureInsideChamberAfterStep(float previousChamberPressure, BottomEnd bottomEndSpecs, 
        float currentCrankPosition, float currentTemp, float previousCrankPosition)
    {
        float previousVolume = GetCurrentChamberVolume(bottomEndSpecs, previousCrankPosition);
        float currentVolume = GetCurrentChamberVolume(bottomEndSpecs, currentCrankPosition);

        float previousChamberDensity = GasHelperMethods.GetAirDensityAtPressure(previousChamberPressure, currentTemp);
        float currentMass = previousVolume * previousChamberDensity;

        currentTemp = GasHelperMethods.GetAirTempAfterDeltaVolume(previousVolume, currentVolume, currentTemp, currentMass);
        
        float currentChamberDensity = currentMass / currentVolume;
        float currentChamberPressure = GasHelperMethods.GetAirPressureAtDensity(currentChamberDensity, currentTemp);
        return (currentChamberPressure, currentTemp);
    }

    public static float GetCurrentChamberVolume(BottomEnd bottomEndSpecs, float crankPosition)
    {
        float clearance = GetCylinderClearanceVolume(GetDisplacementVolume(bottomEndSpecs), bottomEndSpecs.compressionRatio);
        //Debug.Log(clearance);
        float r = GetCrankRadius(bottomEndSpecs.stroke);
        float l = bottomEndSpecs.rodLength;
        float A = crankPosition;
        float x = (r * Mathf.Cos(A)) + (Mathf.Sqrt((l * l) - (r * r * Mathf.Sin(A) * Mathf.Sin(A))));
        float chamberV = ((r + l) - x) * UnitHelperMethods.RadiusToArea(bottomEndSpecs.bore / 2);
        return chamberV + clearance;
    }

    public static float GetDisplacementVolume(BottomEnd bottomEndSpecs)
    {
        return Mathf.PI * Mathf.Pow((bottomEndSpecs.bore / 2), 2) * bottomEndSpecs.stroke;
    }

    public static (float, float) GetNewDensityOfGasInChamber(float gasMass, BottomEnd bottomEndSpecs, float currentCrankPosition, float currentChamberPressure, float currentTemp, float newGasTemp)
    {
        float currentVolume = GetCurrentChamberVolume(bottomEndSpecs, currentCrankPosition);
        float currentChamberDensity = GasHelperMethods.GetAirDensityAtPressure(currentChamberPressure, currentTemp);

        float currentChamberGasMass = currentChamberDensity * currentVolume;

        currentTemp = (currentTemp * (currentChamberGasMass / (currentChamberGasMass + gasMass))) +
            (newGasTemp * (gasMass / (currentChamberGasMass + gasMass)));

        currentChamberGasMass += gasMass;

        currentChamberDensity = currentChamberGasMass / currentVolume;

        return (currentChamberDensity, currentTemp);
    }

    /// <summary>
    /// Returns pressure in the cylinder at each iteration of the engine cycle.
    /// </summary>
    /// <param name="throttle">Throttle percentage, from 0-1, 1 is equal to full throttle</param>
    /// <param name="fI">Moody's friction factor for the intake system</param>
    /// <param name="fE">Moody's friction factor for the exhaust system</param>
    /// <param name="timing">Timing/Topend specs of engine</param>
    /// <param name="crankPosition">Current crank position in the cycle</param>
    /// <param name="cylinderIndex">Current cylinder we are calculating the pressure for</param>
    /// <param name="currentChamberPressure">Current pressure of the current cylinder</param>
    /// <param name="currentExternalPressure">Current pressure going into the manifold</param>
    /// <param name="gasDensity">Density of gas going into the manifold</param>
    /// <param name="gasViscosity">Viscosity of gas going into the manifold</param>
    /// <param name="bottomEndSpecs">Bottom end specs of the engine</param>
    /// <param name="intakeSystemSpecs">Intake manifold, turbos, supercharger specs of the engine</param>
    /// <param name="exhaustSystemSpecs">Headers, pipes and muffler specs</param>
    /// <param name="currentTemp">Temp of chamber gasses at current iteration</param>
    /// <param name="RPM">Revs Per Minute</param>
    /// <param name="previousCrankPosition">Position of crank in last iteration</param>
    /// <param name="timeStep">Current timestep between iterations</param>
    /// <param name="linearIntakeVelocity">Velocity of gas going through intake port</param>
    /// <param name="linearExhuastVelocity">Velocity of gas going through exhaust</param>
    /// <param name="topEndSpecs">Top end specs of the engine</param>
    /// <param name="airRatio">ratio of "air" to actual atmosphere gas coming into cylinder</param>
    /// <param name="atmosphereTemp">External temp</param>
    /// <returns>Returns a tuple; (pressure, intake gas velocity, data for the data/volume graph.</returns>
    public static (float, float, float, float, float) GetPressureAtCrankPosition(float throttle, float fI, float fE, 
        float crankPosition, int cylinderIndex, float currentChamberPressure, float currentExternalPressure, 
        float gasDensity, float gasViscosity, BottomEnd bottomEndSpecs, IntakeSystem intakeSystemSpecs, ExhaustSystem exhaustSystemSpecs, 
        float currentTemp, float RPM, float previousCrankPosition, float timeStep, 
        float linearIntakeVelocity, float linearExhuastVelocity, TopEnd topEndSpecs, float airRatio, float atmosphereTemp)
    {
        float data1 = 0;

        //Get the current stroke/s from the crank position
        (int, string)[] strokes = GetCurrentStrokes(topEndSpecs, UnitHelperMethods.RadiansToDegrees(crankPosition));

        //Get pressure after step as volume changes
        (float, float) pressureTemp = GetPressureInsideChamberAfterStep(currentChamberPressure, bottomEndSpecs, 
            crankPosition, currentTemp, previousCrankPosition);
        currentChamberPressure = pressureTemp.Item1;
        //SHOULD CHECK FOR TEMP LOSS TO CYLINDER
        currentTemp = pressureTemp.Item2;
        
        //Calculate the pressure from the current strokes at this stage of the cycle
        for (int i = 0; i < strokes.Length; i++)
        {
            //Intake stroke
            if (strokes[i].Item1 == 1)
            {
                (float, float, float, float) intakeFlow = GetPressureAtIntakeStroke(throttle, fI, cylinderIndex, crankPosition, currentChamberPressure, 
                    currentExternalPressure, gasDensity, gasViscosity, bottomEndSpecs, intakeSystemSpecs, 
                    currentTemp, RPM, previousCrankPosition, linearIntakeVelocity, timeStep, topEndSpecs, atmosphereTemp);
                currentChamberPressure = intakeFlow.Item1;
                linearIntakeVelocity = intakeFlow.Item2;
                data1 = intakeFlow.Item3;
                currentTemp = intakeFlow.Item4;
            }
            //Compression stroke
            else if (strokes[i].Item1 == 2)
            {
                currentChamberPressure = GetPressureAtCompressionStroke(currentChamberPressure);
            }
            //Combustion stroke
            else if (strokes[i].Item1 == 3)
            {
                currentChamberPressure = GetPressureAtCombustionStroke(currentChamberPressure);
            }
            //Exhaust stroke
            else if (strokes[i].Item1 == 4)
            {
                (float, float, float, float) exhuastFlow = GetPressureAtExhaustStroke(fE, cylinderIndex, crankPosition, currentChamberPressure,
                    currentExternalPressure, gasDensity, gasViscosity, bottomEndSpecs, exhaustSystemSpecs,
                    currentTemp, linearExhuastVelocity, timeStep, topEndSpecs);
                currentChamberPressure = exhuastFlow.Item1;
                linearExhuastVelocity = exhuastFlow.Item2;
                data1 = exhuastFlow.Item3;
            }
        }
        return (currentChamberPressure, linearIntakeVelocity, linearExhuastVelocity, data1, currentTemp);
    }

    //WIP
    private static (float, float, float, float) GetPressureAtIntakeStroke(float throttle, float f, int cylinderIndex, 
        float crankPosition, float currentChamberPressure, float currentExternalPressure, float gasDensity, 
        float gasViscosity, BottomEnd bottomEndSpecs, IntakeSystem intakeSystemSpecs, 
        float currentTemp, float RPM, float previousCrankPosition, float currentLinearIntakeVelocity, float timeStep, 
        TopEnd topEndSpecs, float atmosphereTemp)
    {
        //Data collection for data/volume graph.
        float data1;

        //Get manifold specs
        float averageCrossSectionalAreaOfManifold = 
            GetIntakeCrossSectionalArea(intakeSystemSpecs.intakeSystemSharedVolume + 
            intakeSystemSpecs.intakeSystemIndividualVolume[cylinderIndex], 
            intakeSystemSpecs.intakeSystemLength[cylinderIndex]);
        float aveDiameterOfManifold = UnitHelperMethods.AreaToRadius(averageCrossSectionalAreaOfManifold) * 2;

        float pressureDifferential = currentExternalPressure - currentChamberPressure;
        float gasAccelForce = pressureDifferential * averageCrossSectionalAreaOfManifold;

        //Reynolds number, it determines whether the fluid is undergoing turbulent or laminar flow. 
        //It should always be turbulent in this case so this is unneeded however it is nice to have.
        float Re;
        Re = FluidDynamicsHelperMethods.GetReynoldsNumber(Mathf.Abs(currentLinearIntakeVelocity), gasDensity, gasViscosity, aveDiameterOfManifold);

        //Friction force through the manifold.
        float frictionForce = GasHelperMethods.GetFrictionForceGivenVelocity(f, gasDensity, 
            currentLinearIntakeVelocity, intakeSystemSpecs.intakeSystemLength[cylinderIndex], 
            aveDiameterOfManifold) * averageCrossSectionalAreaOfManifold;

        //Correcting so friction actually makes a difference, unsure if this is correct way to do this
        //I need to figure out a better way of doing this cause this is janky af
        gasAccelForce = gasAccelForce - frictionForce;

        float gasMassInManifold = (intakeSystemSpecs.intakeSystemSharedVolume +
            intakeSystemSpecs.intakeSystemIndividualVolume[cylinderIndex]) * gasDensity;

        //F = MA
        float gasAcceleration = gasAccelForce / gasDensity;
        //Accel lost due to friction
        float gasAccelLost = frictionForce / gasDensity;

        //Send to data/volume graph
        data1 = frictionForce * 1000;

        //V = a * t
        currentLinearIntakeVelocity += (gasAcceleration * timeStep) * throttle;
        //Vel lost due to friction
        float linearVelLost = (gasAccelLost * timeStep) + ((gasAcceleration * timeStep) * (1 - throttle));

        currentLinearIntakeVelocity -= linearVelLost;

        //Get the amount of gass coming into the chamber.
        float volumetricIntakeVelocity = currentLinearIntakeVelocity * averageCrossSectionalAreaOfManifold;
        float intakeGasVolume = volumetricIntakeVelocity * timeStep;
        float intakeGasMass = intakeGasVolume * gasDensity;

        //Get the tempreature increase from friction in manifold
        float volumetricIntakeVelLost = linearVelLost * averageCrossSectionalAreaOfManifold;
        float kineticEnergyLost = 0.5f * gasMassInManifold * volumetricIntakeVelLost * volumetricIntakeVelLost;
        float heatEnergyGained = kineticEnergyLost;
        float specificHeatCapacityOfAir = 1.006f; //This should change with temp and pressure and humidity but I dont need that much accuracy
        float aveTempGained = Mathf.Abs(heatEnergyGained / (gasMassInManifold * specificHeatCapacityOfAir));
        
        float intakeGasTemp = atmosphereTemp + aveTempGained;

        //Use that to calculate the new density of gas in the chamber and therefore pressure
        (float, float) densityTemp = GetNewDensityOfGasInChamber(intakeGasMass, bottomEndSpecs, crankPosition, currentChamberPressure, currentTemp, intakeGasTemp);
        float currentChamberGasDensity = densityTemp.Item1;
        currentTemp = densityTemp.Item2;

        currentChamberPressure = GasHelperMethods.GetAirPressureAtDensity(currentChamberGasDensity, currentTemp);

        return (currentChamberPressure, currentLinearIntakeVelocity, data1, currentTemp);
    }

    private static float GetPressureAtCompressionStroke(float currentChamberPressure)
    {
        return currentChamberPressure;
    }

    private static float GetPressureAtCombustionStroke(float currentChamberPressure)
    {
        return currentChamberPressure + 10000; 
    }

    private static (float, float, float, float) GetPressureAtExhaustStroke(float f, int cylinderIndex,
        float crankPosition, float currentChamberPressure, float currentExternalPressure, float gasDensity,
        float gasViscosity, BottomEnd bottomEndSpecs, ExhaustSystem exhaustSystemSpecs,
        float currentTemp, float currentLinearExhaustVelocity, float timeStep,
        TopEnd topEndSpecs)
    {
        //Data collection for data/volume graph.
        float data1;

        //Get exhaust specs
        float totalLengthOfExhaust = exhaustSystemSpecs.exhaustPipeLength[exhaustSystemSpecs.cylinderIndexToPipe[cylinderIndex]] + exhaustSystemSpecs.headerLength[cylinderIndex];
        float volumeOfExhaust = Mathf.PI * Mathf.Pow(exhaustSystemSpecs.exhaustPipeDiameter / 2, 2) * exhaustSystemSpecs.exhaustPipeLength[exhaustSystemSpecs.cylinderIndexToPipe[cylinderIndex]]
            + Mathf.PI * Mathf.Pow(exhaustSystemSpecs.headerDiameter / 2, 2) * exhaustSystemSpecs.headerLength[cylinderIndex];
        float averageCrossSectionalAreaOfExhaust = volumeOfExhaust / (exhaustSystemSpecs.headerLength[cylinderIndex] + exhaustSystemSpecs.exhaustPipeLength[exhaustSystemSpecs.cylinderIndexToPipe[cylinderIndex]]);
        float averageDiameterOfExhaust = UnitHelperMethods.AreaToRadius(averageCrossSectionalAreaOfExhaust) * 2;

        float pressureDifferential = currentExternalPressure - currentChamberPressure;

        float gasAccelForce = pressureDifferential * averageCrossSectionalAreaOfExhaust;

        //Reynolds number, it determines whether the fluid is undergoing turbulent or laminar flow. 
        //It should always be turbulent in this case so this is unneeded however it is nice to have.
        float Re;
        Re = FluidDynamicsHelperMethods.GetReynoldsNumber(Mathf.Abs(currentLinearExhaustVelocity), gasDensity, gasViscosity, averageDiameterOfExhaust);

        //Friction force through the exhaust.
        float frictionForce = GasHelperMethods.GetFrictionForceGivenVelocity(f, gasDensity, 
            currentLinearExhaustVelocity, totalLengthOfExhaust, averageDiameterOfExhaust);

        //Correcting so friction actually makes a difference, unsure if this is correct way to do this
        //I need to figure out a better way of doing this cause this is janky af
        //gasAccelForce = gasAccelForce - frictionForce;

        float gasMassInExhaust = volumeOfExhaust * gasDensity;

        //F = MA
        float gasAcceleration = gasAccelForce / gasDensity;
        //Accel lost due to friction
        float gasAccelLost = frictionForce / gasDensity;

        //Send to data/volume graph
        data1 = frictionForce * 1000;

        //V = a * t
        currentLinearExhaustVelocity += gasAcceleration * timeStep;
        //Vel lost due to friction
        float linearVelLost = gasAccelLost * timeStep * 100;

        Debug.Log(gasAcceleration.ToString() + " " + gasAccelLost.ToString());

        if (currentLinearExhaustVelocity > 0)
        {
            currentLinearExhaustVelocity -= linearVelLost;
        }
        else
        {
            currentLinearExhaustVelocity += linearVelLost;
        }

        //Get the amount of gass going out of the chamber.
        float volumetricExhaustVelocity = currentLinearExhaustVelocity * averageCrossSectionalAreaOfExhaust;
        float exhaustGasVolume = volumetricExhaustVelocity * timeStep;
        float exhaustGasMass = exhaustGasVolume * gasDensity;

        //Get the tempreature increase from friction in exhaust
        float volumetricExhaustVelLost = linearVelLost * averageCrossSectionalAreaOfExhaust;
        float kineticEnergyLost = 0.5f * gasMassInExhaust * volumetricExhaustVelLost * volumetricExhaustVelLost;
        float heatEnergyGained = kineticEnergyLost;
        float specificHeatCapacityOfAir = 1.006f; //This should change with temp and pressure and humidity but I dont need that much accuracy
        float aveTempGained = Mathf.Abs(heatEnergyGained / (gasMassInExhaust * specificHeatCapacityOfAir));

        float exhaustGasTemp = currentTemp + aveTempGained;

        //Use that to calculate the new density of gas in the chamber and therefore pressure
        (float, float) densityTemp = GetNewDensityOfGasInChamber(exhaustGasMass, bottomEndSpecs, crankPosition, currentChamberPressure, currentTemp, currentTemp);
        float currentChamberGasDensity = densityTemp.Item1;

        currentChamberPressure = GasHelperMethods.GetAirPressureAtDensity(currentChamberGasDensity, currentTemp);

        return (currentChamberPressure, currentLinearExhaustVelocity, data1, currentTemp);
    }
}
