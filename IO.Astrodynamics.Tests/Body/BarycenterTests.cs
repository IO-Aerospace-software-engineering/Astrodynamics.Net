using IO.Astrodynamics.Body;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;

namespace IO.Astrodynamics.Tests.Body;

public class BarycenterTests
{
    public BarycenterTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }
    [Fact]
    public void Create()
    {
        var ssb = new Barycenter(Barycenters.SOLAR_SYSTEM_BARYCENTER.NaifId);
        Assert.Equal("SOLAR SYSTEM BARYCENTER", ssb.Name);
        Assert.Equal(0, ssb.NaifId);
        Assert.Equal(1.9910779956942692E+30,ssb.Mass);
        Assert.Null(ssb.Frame);
    }
}