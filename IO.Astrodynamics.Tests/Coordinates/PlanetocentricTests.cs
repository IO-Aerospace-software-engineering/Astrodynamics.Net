// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Math;
using Xunit;

namespace IO.Astrodynamics.Tests.Coordinates;

public class PlanetocentricTests
{
    [Fact]
    public void ToCartesian()
    {
        var plan = new Planetocentric(-98.34959789 * Astrodynamics.Constants.Deg2Rad, -18.26566077 * Astrodynamics.Constants.Deg2Rad, 403626339.12495);
        var res = plan.ToCartesianCoordinates();
        Assert.Equal(new Vector3(-55658443.24257991, -379226329.314363, -126505930.63558689), res);
    }
}