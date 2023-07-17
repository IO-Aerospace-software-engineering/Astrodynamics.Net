using System;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Body;

public class LagrangePoint : CelestialItem
{
    public LagrangePoint(NaifObject systemObject) : base(systemObject.NaifId, systemObject.Name, 0.0, null)
    {
        this.InitialOrbitalParameters = GetEphemeris(DateTimeExtension.J2000, new Barycenter(Barycenters.EARTH_BARYCENTER.NaifId), Frame.ECLIPTIC_J2000, Aberration.None);
    }
}