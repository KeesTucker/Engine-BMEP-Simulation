using UnityEngine;

public static class UnitHelperMethods
{
    public static float CelsiusToKelvin(float T)
    {
        return T + 273.15f;
    }
    public static float KelvinToCelsius(float T)
    {
        return T - 273.15f;
    }

    public static float Cm3ToM3(float V)
    {
        return V * 0.000001f;
    }

    public static float M3toCm3(float V)
    {
        return V / 0.000001f;
    }

    public static float RadiansToDegrees(float A)
    {
        return A * 180 / Mathf.PI;
    }

    public static float DegreesToRadians(float A)
    {
        return A / (180 / Mathf.PI);
    }

    public static float AreaToRadius(float A)
    {
        return Mathf.Sqrt(A / Mathf.PI);
    }

    public static float RadiusToArea(float r)
    {
        return Mathf.PI * r * r;
    }

    public static float PaToMmHg(float P)
    {
        return P / 133.322f;
    }
}
