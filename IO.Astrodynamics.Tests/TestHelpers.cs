using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Physics;
using IO.Astrodynamics.SolarSystemObjects;

namespace IO.Astrodynamics.Tests
{
    internal static class TestHelpers
    {
        internal static CelestialBody Sun => new(Stars.Sun);

        internal static CelestialBody Earth => new(PlanetsAndMoons.EARTH, Frames.Frame.ICRF, new DateTime(2021, 1, 1), atmosphericModel: new EarthAtmosphericModel());

        internal static CelestialBody Moon => new(PlanetsAndMoons.MOON, Frames.Frame.ICRF, new DateTime(2021, 1, 1));

        internal static CelestialBody EarthAtJ2000 => new(PlanetsAndMoons.EARTH, Frames.Frame.ICRF, new DateTime(2000, 1, 1, 12, 0, 0));
        internal static CelestialBody EarthWithAtmAndGeoAtJ2000 => new(PlanetsAndMoons.EARTH, Frames.Frame.ICRF, new DateTime(2000, 1, 1, 12, 0, 0),
            new GeopotentialModelParameters("Data/SolarSystem/EGM2008_to70_TideFree"), new EarthAtmosphericModel());

        internal static CelestialBody MoonAtJ2000 => new(PlanetsAndMoons.MOON, Frames.Frame.ICRF, new DateTime(2000, 1, 1, 12, 0, 0));

        internal static CelestialBody MoonAt20011214 => new(PlanetsAndMoons.MOON, Frames.Frame.ICRF, new DateTime(2001, 12, 14, 0, 0, 0));
        private static object LockObj = new object();

        internal static bool VectorComparer(Vector3 v1, Vector3 v2)
        {
            lock (LockObj)
            {
                return System.Math.Abs(v1.X - v2.X) < 1E-03 && System.Math.Abs(v1.Y - v2.Y) < 1E-03 && System.Math.Abs(v1.Z - v2.Z) < 1E-03;
            }
        }
    }
}