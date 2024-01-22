using System.IO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Propagator.Forces;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Body;

public class GravitationalAccelerationTest
{
    public GravitationalAccelerationTest()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void ComputeGeopotentialGravityAcceleration()
    {
        GeopotentialGravitationalField gravity =
            new GeopotentialGravitationalField(new FileInfo(Path.Combine(Constants.SolarSystemKernelPath.ToString(), "EGM2008_to70_TideFree")));
        StateVector parkingOrbit = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.EarthAtJ2000, DateTimeExtension.J2000,
            Frames.Frame.ICRF);
        var res = gravity.ComputeGravitationalAcceleration(parkingOrbit);
        Assert.Equal(new Vector3(-8.621408090488707, -8.881784197001252E-16, 5.421010862427522E-20), res);
    }

    [Fact]
    public void ComputeGravityAcceleration()
    {
        GravitationalField gravity = new GravitationalField();
        StateVector parkingOrbit = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.EarthAtJ2000, DateTimeExtension.J2000,
            Frames.Frame.ICRF);
        var res = gravity.ComputeGravitationalAcceleration(parkingOrbit);
        Assert.Equal(new Vector3(-8.620251631403459, 0.0, 0.0), res);
    }
}