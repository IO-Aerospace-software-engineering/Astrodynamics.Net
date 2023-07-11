using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.SolarSystemObjects;

namespace IO.Astrodynamics.Tests
{
    internal static class TestHelpers
    {
        internal static CelestialBody Sun => new(Stars.Sun);

        internal static CelestialBody Earth => new(PlanetsAndMoons.EARTH, Frames.Frame.ICRF, new DateTime(2021, 1, 1));

        internal static CelestialBody Moon => new(PlanetsAndMoons.MOON, Frames.Frame.ICRF, new DateTime(2021, 1, 1));

        internal static CelestialBody EarthAtJ2000 => new(PlanetsAndMoons.EARTH, Frames.Frame.ICRF, new DateTime(2000, 1, 1, 12, 0, 0));

        internal static CelestialBody MoonAtJ2000 => new(PlanetsAndMoons.MOON, Frames.Frame.ICRF, new DateTime(2000, 1, 1, 12, 0, 0));

        internal static CelestialBody MoonAt20011214 => new(PlanetsAndMoons.MOON, Frames.Frame.ICRF, new DateTime(2001, 12, 14, 0, 0, 0));
    }
}