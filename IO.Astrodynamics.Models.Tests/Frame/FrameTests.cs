using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Frame;

public class FrameTests
{
    [Fact]
    public void CreateFrame()
    {
        IO.Astrodynamics.Models.OrbitalParameters.StateOrientation so = new IO.Astrodynamics.Models.OrbitalParameters.StateOrientation(new Quaternion(1.0, 2.0, 3.0, 4.0), new Vector3(5.0, 6.0, 7.0), DateTime.MinValue, IO.Astrodynamics.Models.Frame.Frame.ICRF);
        IO.Astrodynamics.Models.Frame.Frame frame = new IO.Astrodynamics.Models.Frame.Frame("frm", so);
        Assert.Equal("frm", frame.Name);
        Assert.Equal(DateTime.MinValue, frame.FromICRF(DateTime.MinValue).Epoch);
        Assert.Equal(so, frame.ToICRF(DateTime.MinValue));
        var expectedFrame = new IO.Astrodynamics.Models.OrbitalParameters.StateOrientation(new Quaternion(1.0, 2.0, 3.0, 4.0).Conjugate(), new Vector3(5.0, 6.0, 7.0).Inverse(), DateTime.MinValue, IO.Astrodynamics.Models.Frame.Frame.ICRF);
        Assert.Equal(expectedFrame, frame.FromICRF(DateTime.MinValue));
    }

    [Fact]
    public void ToInertialFrame()
    {
        var so = IO.Astrodynamics.Models.Frame.Frame.ICRF.ToFrame(IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC, DateTime.MinValue);
        Assert.Equal(so, new StateOrientation(new Quaternion(0.97915322, new Vector3(-0.20312304, 0.0, 0.0)), Vector3.Zero, DateTime.MinValue, IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC));
    }

    [Fact]
    public void ToNonInertialFrame()
    {
        var epoch = DateTime.MinValue;
        var moonFrame = new IO.Astrodynamics.Models.Frame.Frame("moon", new StateOrientation(new Quaternion(0.92408921, 0.19537823, -0.07960816, 0.31866756), new Vector3(5.89144380E-08, 1.09616810E-06, -2.42504180E-06), epoch, IO.Astrodynamics.Models.Frame.Frame.ICRF));
        var earthFrame = new IO.Astrodynamics.Models.Frame.Frame("earth", new StateOrientation(new Quaternion(0.76686839, 0.0, 0.0, -0.64180439), new Vector3(-1.00974196E-28, -3.08055237E-12, -7.29211502E-05), epoch, IO.Astrodynamics.Models.Frame.Frame.ICRF));
        var q = moonFrame.ToFrame(earthFrame, epoch);

        Assert.Equal(epoch, q.Epoch);
        Assert.Equal(earthFrame, q.Frame);
        Assert.Equal(new Quaternion(0.50413256573048337, 0.20092225524897211, 0.064345624234367307, 0.83746059041206034), q.Orientation);
        Assert.Equal(new Vector3(5.8914438E-08, 1.0961711805523701E-06, 7.0496108399999992E-05), q.AngularVelocity);
        Assert.Equal(7.050465489393326E-05, q.AngularVelocity.Magnitude());

    }

}