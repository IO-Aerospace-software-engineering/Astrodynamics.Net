using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Frame;

public class FrameTests
{
    [Fact]
    public void ToInertialFrame()
    {
        var so = Frames.Frame.ICRF.ToFrame(Frames.Frame.ECLIPTIC, DateTime.MinValue);
        Assert.Equal(so, new StateOrientation(new Quaternion(0.97915322, new Vector3(-0.20312304, 0.0, 0.0)), Vector3.Zero, DateTime.MinValue, Frames.Frame.ECLIPTIC));
    }

    [Fact]
    public void ToNonInertialFrame()
    {
        var epoch = DateTime.MinValue;
        var moonFrame = new Frames.Frame(PlanetsAndMoons.MOON.Frame);
        var earthFrame = new Frames.Frame(PlanetsAndMoons.EARTH.Frame);
        var q = moonFrame.ToFrame(earthFrame, epoch);

        Assert.Equal(epoch, q.Epoch);
        Assert.Equal(earthFrame, q.Frame);
        Assert.Equal(new Quaternion(0.50413256573048337, 0.20092225524897211, 0.064345624234367307, 0.83746059041206034), q.Orientation);
        Assert.Equal(new Vector3(5.8914438E-08, 1.0961711805523701E-06, 7.0496108399999992E-05), q.AngularVelocity);
        Assert.Equal(7.050465489393326E-05, q.AngularVelocity.Magnitude());

    }

}