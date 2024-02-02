// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Propagator.Forces;
using IO.Astrodynamics.Propagator.Integrators;
using IO.Astrodynamics.Time;
using Xunit;
using Vector3 = IO.Astrodynamics.Math.Vector3;

namespace IO.Astrodynamics.Tests.Propagators.Integrators;

public class IntegratorTests
{
    public IntegratorTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void IntegrateWithPerturbations()
    {
        var sun = TestHelpers.Sun;
        var moon = TestHelpers.MoonAtJ2000;
        var earth = TestHelpers.EarthWithAtmAndGeoAtJ2000;
        Clock clk = new Clock("My clock", 1.0 / 256.0);
        Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 100.0, 10000.0, clk,
            new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), earth, DateTimeExtension.J2000, Frames.Frame.ICRF));

        List<ForceBase> forces = new List<ForceBase>();
        forces.Add(new GravitationalAcceleration(sun));
        forces.Add(new GravitationalAcceleration(moon));
        forces.Add(new GravitationalAcceleration(earth));
        forces.Add(new AtmosphericDrag(spc));
        forces.Add(new SolarRadiationPressure(spc));
        VVIntegrator vvIntegrator = new VVIntegrator(forces, TimeSpan.FromSeconds(1.0), spc.InitialOrbitalParameters.ToStateVector());
        TimeSpan deltaT = TimeSpan.FromSeconds(1.0);
        StateVector[] data = new StateVector[2];
        Array.Fill(data, spc.InitialOrbitalParameters.ToStateVector(), 0, 2);
        vvIntegrator.Integrate(data, 1);
        Assert.Equal(new Vector3(6799995.689837334, 7656.2176410373559, -0.001202574270660037), data[1].Position);
        Assert.Equal(new Vector3(-8.620322605388086, 7656.210010294659, -0.0024051475080610115), data[1].Velocity);
    }
}