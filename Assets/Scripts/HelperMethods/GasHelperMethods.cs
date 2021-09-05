using UnityEngine;

public static class GasHelperMethods
{
    /// <summary>
    /// Get air density provided with air pressure
    /// </summary>
    /// <param name="pressure">Pressure in Pascals</param>
    /// <param name="airTemp">Air temp in celsius</param>
    /// <returns>Returns density in kg/m^3</returns>
    public static float GetAirDensityAtPressure(float pressure, float airTemp)
    {
        float Rsp = 287.052f; //Specific gas constant

        float T = airTemp;
        float P = pressure;
        float p;
        p = P / (Rsp * T);
        return p;
    }

    public static float GetAirPressureAtDensity(float density, float airTemp)
    {
        float Rsp = 287.052f; //Specific gas constant
        float T = airTemp;//UnitHelperMethods.CelsiusToKelvin(airTemp);
        float p = density;
        float P;
        P = p * (Rsp * T);
        return P;

    }

    /// <summary>
    /// Get air viscosity provided with an air density. This is super janky and only somewhat accurate [WIP]
    /// </summary>
    /// <param name="density">Density in kg/m^3</param>
    /// <returns>Returns viscosity in kg/m/s</returns>
    public static float GetAirViscosityAtDensity(float density)
    {
        float p = density;

        //Absolutley cancer but I couldn't find a formula online, this somewhat works for now:
        float u = Mathf.Pow((1f / p) * 2.6f, 0.78f) * Mathf.Pow(10, -5);

        return u;
    }

    public static float GetFrictionForceGivenVelocity(float coefficentOfFriction, float density, float velocity, float length, float diameter)
    {
        float deltaP;
        deltaP = (coefficentOfFriction * density * velocity * velocity * length) / (2 * diameter);
        return deltaP;
    }

    public static float GetGasVelGivenDelataP(float deltaP, float diameter, float density, float length, float frictionCoefficent)
    {
        float v;
        v = Mathf.Sqrt((deltaP * 2 * diameter) / (length * density * frictionCoefficent));
        return v;
    }

    public static float GetAirTempAfterDeltaVolume(float previousVolume, float currentVolume, float currentTemp, float currentMassOfAir)
    {
        float r = currentVolume / previousVolume;
        float n = 5f / 7f;

        currentTemp *= Mathf.Pow(r, 1f - (1f / n));
        return currentTemp;
    }
}
