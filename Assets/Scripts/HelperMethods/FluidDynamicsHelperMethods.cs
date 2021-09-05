public static class FluidDynamicsHelperMethods
{
    /// <summary>
    /// Gets linear velocity of fluid through a hole/pipe
    /// </summary>
    /// <param name="volumeVelocity">Volume velocity in m^3/s</param>
    /// <param name="orificeAverageCrossSectionalArea">Average cross sectional area of the hole/pipe</param>
    /// <returns>Returns average linear spead of fluid in m/s</returns>
    public static float GetAverageLinearVelocityOfFluidThroughOrifice(float volumeVelocity, float orificeAverageCrossSectionalArea)
    {
        float vV = volumeVelocity;
        float A = orificeAverageCrossSectionalArea;

        float vl = vV / A;

        return vl;
    }

    /// <summary>
    /// Gets Reynolds number of a given fluid moving at a velocity through a pipe
    /// </summary>
    /// <param name="velocity">Velocity of fluid in m/s</param>
    /// <param name="density">Density of fluid in kg/m^3</param>
    /// <param name="viscosity">Viscosity of fluid in S.I unitys</param>
    /// <param name="diameter">Diameter of pipe in m</param>
    /// <returns>Returns Reynold's number</returns>
    public static float GetReynoldsNumber(float velocity, float density, float viscosity, float diameter)
    {
        float Re;
        Re = diameter * velocity * density / viscosity;
        return Re;
    }
}
