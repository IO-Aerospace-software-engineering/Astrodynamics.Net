using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Body;

public class StarTests
{
    [Fact]
    public void Create()
    {
        var star = new Star(1, "spec", 2, 0.3792, new Equatorial(10, 20), 4, 5, 6, 7, 8, 9, DateTimeExtension.J2000);
        Assert.Equal(1, star.CatalogNumber);
        Assert.Equal("spec", star.SpectralType);
        Assert.Equal(2, star.VisualMagnitude);
        Assert.Equal(DateTimeExtension.J2000, star.Epoch);
        Assert.Equal(new Equatorial(10, 20), star.EquatorialCoordinatesAtEpoch);
        Assert.Equal(4, star.DeclinationProperMotion);
        Assert.Equal(5, star.RightAscensionProperMotion);
        Assert.Equal(6, star.DeclinationSigma);
        Assert.Equal(7, star.RightAscensionSigma);
        Assert.Equal(8, star.DeclinationSigmaProperMotion);
        Assert.Equal(9, star.RightAscensionSigmaProperMotion);
        Assert.Equal(0.3792, star.Parallax);
    }

    [Fact]
    public void GetEquatorialCoordinates()
    {
        var star = new Star(1, "spec", 2, 0.3792, new Equatorial(10, 20), 4, 5, 6, 7, 8, 9, DateTimeExtension.J2000);
        var res = star.GetEquatorialCoordinates(new DateTime(2001, 1, 1, 12, 0, 0));
        Assert.Equal(new Equatorial(14.008213552361397, 25.010266940451746, 2.6371308016877637), res);
    }

    [Fact]
    public void GetDeclinationSigma()
    {
        var star = new Star(1, "spec", 2, 0.3792, new Equatorial(10, 20), 4, 5, 6, 7, 8, 9, DateTimeExtension.J2000);
        var res = star.GetDeclinationSigma(new DateTime(2001, 1, 1, 12, 0, 0));
        Assert.Equal(10.013146534697984, res);
    }

    [Fact]
    public void GetRightAscensionSigma()
    {
        var star = new Star(1, "spec", 2, 0.3792, new Equatorial(10, 20), 4, 5, 6, 7, 8, 9, DateTimeExtension.J2000);
        var res = star.GetRightAscensionSigma(new DateTime(2001, 1, 1, 12, 0, 0));
        Assert.Equal(11.416347506941577, res);
    }
    
}