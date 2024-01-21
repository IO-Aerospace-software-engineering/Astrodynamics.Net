using System.IO;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Propagator.Forces;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Propagators.Integrators.Forces;

public class GravityTest
{
    public GravityTest()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void ComputeGravityForce()
    {
        GravitationalField gravity = new GravitationalField(new FileInfo(Path.Combine(Constants.SolarSystemKernelPath.ToString(), "EGM2008_to70_TideFree")));
        StateVector parkingOrbit = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.EarthAtJ2000, DateTimeExtension.J2000,
            Frames.Frame.ICRF);
        var res = gravity.ComputeGravitationalAcceleration(parkingOrbit);
        Assert.Equal(new Vector3(-8.6214171207929855, 0.0, 0.0), res);
    }
}