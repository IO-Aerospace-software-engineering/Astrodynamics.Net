// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Linq;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
using Xunit;
using CelestialBody = IO.Astrodynamics.Body.CelestialBody;
using Spacecraft = IO.Astrodynamics.Body.Spacecraft.Spacecraft;
using StateVector = IO.Astrodynamics.OrbitalParameters.StateVector;
using Window = IO.Astrodynamics.Time.Window;

namespace IO.Astrodynamics.Tests.Propagators;

public class SpacecraftPropagatorTests
{
    public SpacecraftPropagatorTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void CheckSymplecticProperty()
    {
        var earth = new CelestialBody(399);
        Clock clk = new Clock("My clock", 256);
        var orbit = new KeplerianElements(150000000000.0, 0, 0, 0, 0, 0, TestHelpers.Sun, DateTimeExtension.J2000, Frames.Frame.ICRF);
        Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 100.0, 10000.0, clk, orbit);
        Propagator.SpacecraftPropagator spacecraftPropagator = new Propagator.SpacecraftPropagator(new Window(DateTimeExtension.J2000, DateTimeExtension.J2000.AddHours(4.0)), spc,
            null, false, false, TimeSpan.FromSeconds(1.0));
        var res = spacecraftPropagator.Propagate();
        var energy = res.stateVectors.Select(x => x.SpecificOrbitalEnergy());
        var min = energy.Min();
        var max = energy.Max();
        var diff = max - min;
        Assert.True(diff < 2.6E-06);
    }
}