// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Linq;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Time;
using Xunit;
using CelestialBody = IO.Astrodynamics.Body.CelestialBody;
using Spacecraft = IO.Astrodynamics.Body.Spacecraft.Spacecraft;
using StateVector = IO.Astrodynamics.OrbitalParameters.StateVector;
using Window = IO.Astrodynamics.Time.Window;

namespace IO.Astrodynamics.Tests.Propagators;

public class PropagatorTests
{
    public PropagatorTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void CheckSymplecticProperty()
    {
        var earth = new CelestialBody(399);
        Clock clk = new Clock("My clock", 256);
        Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 100.0, 10000.0, clk,
            new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), earth, DateTimeExtension.J2000, Frames.Frame.ICRF));
        Propagator.Propagator propagator = new Propagator.Propagator(new Window(DateTimeExtension.J2000, DateTimeExtension.J2000.AddHours(4.0)), spc, [earth], false, false,
            TimeSpan.FromSeconds(1.0));
        var res = propagator.Propagate();
        Assert.True(System.Math.Abs(res.Max(x => x.SpecificOrbitalEnergy()) - res.Min(x => x.SpecificOrbitalEnergy())) < 1.2E-05);
    }
}