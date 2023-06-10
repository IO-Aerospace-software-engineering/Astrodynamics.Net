using System;
using IO.Astrodynamics.Models.Body;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Body;

public class CelestialBodyTests
{
    [Fact]
    public void Create()
    {
        CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
        Assert.Equal(399, earth.NaifId);
        Assert.Equal("earth", earth.Name);
        Assert.Equal("IAU_EARTH", earth.FrameName);
        Assert.Equal(3.986004418E+5, earth.GM);
        Assert.Equal(5.972168494074285E+24, earth.Mass);
        Assert.Equal(6356.7519, earth.PolarRadius);
        Assert.Equal(6378.1366, earth.EquatorialRadius);
        Assert.Equal(0.0033528131084554717, earth.Flatenning);
    }

    [Fact]
    public void CreateExceptions()
    {
        Assert.Throws<ArgumentException>(() => new CelestialBody(-399, "earth", 3.986004418E+5, 6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "", 3.986004418E+5, 6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "earth", -3.986004418E+5, 6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "earth", 3.986004418E+5, -6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, -6378.1366));
    }

}