using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Tests
{
    internal static class TestHelpers
    {
        internal static CelestialBody Sun { get; } = new(10);

        internal static CelestialBody Earth { get; } = new(399, Frames.Frame.ICRF, new DateTime(2021, 1, 1));

        internal static CelestialBody Moon { get; } = new(301, Frames.Frame.ICRF, new DateTime(2021, 1, 1));

        internal static CelestialBody EarthAtJ2000 { get; } = new(399, Frames.Frame.ICRF, new DateTime(2000, 1, 1, 12, 0, 0));

        internal static CelestialBody MoonAtJ2000 { get; } = new(301, Frames.Frame.ICRF, new DateTime(2000, 1, 1, 12, 0, 0));

        internal static CelestialBody MoonAt20011214 { get; } = new(301, Frames.Frame.ICRF, new DateTime(2001, 12, 14, 0, 0, 0));
    }
}