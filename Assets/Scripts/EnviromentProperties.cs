using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentProperties : MonoBehaviour
{
    [Header("Air Input Properties")]
    public float altitude = 10;
    public float airTemp = 25;
    [Range(0, 1)]
    public float relativeHumidity = 0.2f;

    [Header("Air Output Properties")]
    public float atmospherePressure;
    public float atmosphereDensity;
    public float atmosphereViscosity;
    //This is the actual amount of air in the gas.
    public float airDensityRatio;

    // Update is called once per frame
    void Update()
    {
        UpdateAirProperties();
    }

    public void UpdateAirProperties()
    {
        (float, float, float, float) airProperties = AtmosphericHelperMethods.GetAirProperties(altitude, airTemp, relativeHumidity);
        atmospherePressure = airProperties.Item1;
        atmosphereDensity = airProperties.Item2;
        atmosphereViscosity = airProperties.Item3;
        float absoluteHumidity = airProperties.Item4;
        airDensityRatio = (atmosphereDensity - absoluteHumidity) / atmosphereDensity;
    }
}
