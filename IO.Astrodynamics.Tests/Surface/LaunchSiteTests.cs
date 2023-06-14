using System.Linq;
using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Surface;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Surface
{
    public class LaunchSiteTests
    {
        [Fact]
        public void Create()
        {
            LaunchSite site = new LaunchSite(33,"l1", TestHelpers.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0), new AzimuthRange(1.0, 2.0));
            Assert.Equal("l1", site.Name);
            Assert.Equal(TestHelpers.GetEarthAtJ2000(), site.Body);
            Assert.Equal(new Geodetic(1.0, 2.0, 3.0), site.Geodetic);
            Assert.Single(site.AzimuthRanges);
            Assert.Equal(new AzimuthRange(1.0, 2.0), site.AzimuthRanges.First());
        }


        [Fact]
        public void IsAzimuthAllowed()
        {
            LaunchSite site = new LaunchSite(33,"l1", TestHelpers.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0),new AzimuthRange(1.0, 2.0), new AzimuthRange(4.0, 5.0));
            Assert.True(site.IsAzimuthAllowed(1.0));
            Assert.True(site.IsAzimuthAllowed(5.0));
            Assert.False(site.IsAzimuthAllowed(3.0));
            Assert.False(site.IsAzimuthAllowed(-1.0));
        }
    }
}
