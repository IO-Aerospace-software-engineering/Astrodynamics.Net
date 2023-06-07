using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using IO.Astrodynamics.Models.Time;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Frame;

public class FrameTests
{
    private API _api;
    public FrameTests()
    {
        _api = new API();
        _api.LoadKernels(Astrodynamics.Tests.Constants.SolarSystemKernelPath);
    }
    [Fact]
    public void ToInertialFrame()
    {
        var so = Frames.Frame.ICRF.ToFrame(Frames.Frame.ECLIPTIC, DateTimeExtension.J2000);
        Assert.Equal(so, new StateOrientation(new Quaternion(0.9791532214288993, new Vector3(-0.20312303898231013, 0.0, 0.0)), Vector3.Zero, DateTimeExtension.J2000, Frames.Frame.ICRF));
    }

    [Fact]
    public void ToNonInertialFrame()
    {
        var epoch = DateTimeExtension.J2000;
        var moonFrame = new Frames.Frame(PlanetsAndMoons.MOON.Frame);
        var earthFrame = new Frames.Frame(PlanetsAndMoons.EARTH.Frame);
        var q = moonFrame.ToFrame(earthFrame, epoch);

        Assert.Equal(epoch, q.Epoch);
        Assert.Equal(moonFrame, q.ReferenceFrame);
        Assert.Equal(new Quaternion(0.5044792582956342, 0.2009316556383325, 0.06427003637545137, 0.8372553434503859), q.Rotation);
        Assert.Equal(new Vector3(1.980539178135755E-05, 2.2632012450014214E-05, 6.376864584829888E-05), q.AngularVelocity);
        Assert.Equal(7.050462200789696E-05, q.AngularVelocity.Magnitude());

    }

}