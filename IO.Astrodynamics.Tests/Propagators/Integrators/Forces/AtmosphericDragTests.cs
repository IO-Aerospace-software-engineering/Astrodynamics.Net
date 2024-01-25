using System;
using System.IO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Physics;
using IO.Astrodynamics.Propagator.Forces;
using IO.Astrodynamics.Time;
using Xunit;
using CelestialBody = IO.Astrodynamics.Body.CelestialBody;

namespace IO.Astrodynamics.Tests.Propagators.Integrators.Forces;

public class AtmosphericDragTests
{
    public AtmosphericDragTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void ComputeAcceleration()
    {
        var earth = new CelestialBody(399, Path.Combine(Constants.SolarSystemKernelPath.ToString(), "EGM2008_to70_TideFree"), new EarthAtmosphericModel());
        Clock clk = new Clock("My clock", 1.0 / 256.0);
        Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 100.0, 10000.0, clk,
            new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), earth,
                DateTimeExtension.J2000, Frames.Frame.ICRF),dragCoeff:1.0);
        AtmosphericDrag atmosphericDrag = new AtmosphericDrag(spc);

        StateVector parkingOrbit = new StateVector(new Vector3(7380000.0, 0.0, 0.0), new Vector3(0.0, 9700.0, 0.0), earth, DateTimeExtension.J2000,
            Frames.Frame.ICRF);
        var res = atmosphericDrag.Apply(parkingOrbit);
        Assert.Equal(new Vector3(-8.456677063948622E-09, 4.237795744348693E-08, 1.8372880484798083E-08), res);
    }
}