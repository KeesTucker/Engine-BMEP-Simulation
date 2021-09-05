using UnityEngine;

public static class AtmosphericHelperMethods
{
    /// <summary>
    /// Gets interval atmospheric layer number from altitude
    /// </summary>
    /// <param name="altitude">Altitude in meters</param>
    /// <returns>Returns an int layer number, clamped to 7</returns>
    public static int GetIntervalLayerNumber(float altitude)
    {
        //Atmospheric level definitons in kilometers 
        int[] intervalLayerDefinitions = { 11, 20, 32, 47, 51, 71, 85, 800 };

        int intervalLayerNumber;
        for (intervalLayerNumber = 0; intervalLayerNumber < intervalLayerDefinitions.Length; intervalLayerNumber++)
        {
            if (altitude / 1000f < intervalLayerDefinitions[intervalLayerNumber])
            {
                break;
            }
        }
        intervalLayerNumber = Mathf.Clamp(intervalLayerNumber, 0, 7);
        return intervalLayerNumber;
    }

    /// <summary>
    /// Get name of the current atmospheric level number.
    /// </summary>
    /// <param name="intervalLayerNumber">int representing level number</param>
    /// <returns>Returns a string with level name</returns>
    public static string GetAtmosphericLevelName(int intervalLayerNumber)
    {
        string[] atmosphericLevelNames = { "Troposphere", "Tropopause(Stratosphere I)", "Stratosphere II", "Stratosphere III", "Stratopause(Mesosphere I)", "Mesosphere II", "Mesosphere III", "Thermosphere" };
        return atmosphericLevelNames[intervalLayerNumber];
    }

    /// <summary>
    /// Get the base altitude in kilometers at the current interval layer
    /// </summary>
    /// <param name="intervalLayerNumber">int representing level number</param>
    /// <returns>Returns an int representing altitude in kms</returns>
    public static int GetBaseGeoPotentialAltitudeAboveMeanSeaLevel(int intervalLayerNumber)
    {
        int[] baseGeoPotentialAltitudes = { 0, 11, 20, 32, 47, 51, 71, 85 };
        return baseGeoPotentialAltitudes[intervalLayerNumber];
    }

    /// <summary>
    /// Get pressure at base of interval layer
    /// </summary>
    /// <param name="intervalLayerNumber">int representing level number</param>
    /// <returns>Returns float representing pressure in Pascals</returns>
    public static float GetBaseStaticPressure(int intervalLayerNumber)
    {
        float[] baseStaticPressures = { 101325f, 22632.06f, 5474.889f, 868.0187f, 110.9063f, 66.93887f, 3.956420f, 0.3734f };
        return baseStaticPressures[intervalLayerNumber];
    }

    /// <summary>
    /// Get temp at base of interval layer
    /// </summary>
    /// <param name="intervalLayerNumber">int representing level number</param>
    /// <returns>Returns a float representing temp in Kelvin</returns>
    public static float GetBaseTemperature(int intervalLayerNumber) //Returns in kelvin
    {
        float[] baseTemperatures = { 288.15f, 216.65f, 216.65f, 228.65f, 270.65f, 270.65f, 214.65f, 186.87f };
        return baseTemperatures[intervalLayerNumber];
    }

    /// <summary>
    /// Get change in temp per km at base of interval layer
    /// </summary>
    /// <param name="intervalLayerNumber">int representing level number</param>
    /// <returns>Returns lapse rate in Kelvin/KM</returns>
    public static float GetBaseTemperatureLapseRate(int intervalLayerNumber) //Returns in kelvin/km
    {
        float[] baseTemperatureLapseRates = { -6.5f, 0, 1f, 2.8f, 0, -2.8f, -2f, 0 };
        return baseTemperatureLapseRates[intervalLayerNumber];
    }

    /// <summary>
    /// Gets air pressure provided with enviromental variables
    /// </summary>
    /// <param name="altitude">Current altitude in meters</param>
    /// <param name="airTemp">Current temp in celsius</param>
    /// <param name="relativeHumidity">Current relative humidity from 0-1, convert from % by / 100</param>
    /// <returns>Returns air pressure in Pascals</returns>
    public static float GetAirPressure(float altitude, float airTemp) //In meters, celsius, normalised humidity (0-1) units
    {
        float H = altitude / 1000f; //For some reason H needs to be in kms even though the formula specifies ms
        int b = GetIntervalLayerNumber(altitude);
        int Hb = GetBaseGeoPotentialAltitudeAboveMeanSeaLevel(b);
        float Pb = GetBaseStaticPressure(b);
        float Tb = GetBaseTemperature(b);
        float Lb = GetBaseTemperatureLapseRate(b);
        float R = 8.31432f * Mathf.Pow(10f, -3f); //Universal gas constant in Nm * k/mol/K
        float g0 = Physics.gravity.magnitude; //Gravitational acceleration in m/s^2
        float M = 0.0289644f; //Molar mass of Earth’s air in kg/mol

        float T;
        if (b == 0)
        {
            T = UnitHelperMethods.CelsiusToKelvin(airTemp);
        }
        else
        {
            T = Tb;
        }

        float P; //Pressure
        if (Lb == 0)
        {
            
            P = Pb * Mathf.Exp((-g0 * M * (H - Hb)) / (R * T));
        }
        else
        {
            P = Pb * Mathf.Pow(T / (T + Lb * (H - Hb)), (g0 * M) / (R * Lb));
        }

        return P;
    }

    public static (float, float, float, float) GetAirProperties(float H, float T, float rH)
    {
        float P = GetAirPressure(H, T);
        
        float p = GasHelperMethods.GetAirDensityAtPressure(P, UnitHelperMethods.CelsiusToKelvin(T));
        float aH = GetAbsoluteHumidity(T, rH, P);
        p += aH / 2;
        P = GasHelperMethods.GetAirPressureAtDensity(p, UnitHelperMethods.CelsiusToKelvin(T));

        float u = GasHelperMethods.GetAirViscosityAtDensity(p);
        return (P, p, u, aH);
    }

    public static float GetAbsoluteHumidity(float T, float rH, float P)
    {
        P = UnitHelperMethods.PaToMmHg(P);
        float eW = 6.112f * Mathf.Exp((17.62f * T) / (243.12f + T));
        float fP = 1.0016f + (3.15f * Mathf.Pow(10, -6) * P) - (0.074f * Mathf.Pow(P, -1));
        eW *= fP;

        float e = eW * rH;
        float aH = e / (461.5f * UnitHelperMethods.CelsiusToKelvin(T));
        aH *= 100;
        return aH;
    }
}
