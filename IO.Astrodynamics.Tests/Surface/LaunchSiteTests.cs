using IO.Astrodynamics.Models.Surface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Astrodynamics.Models.Coordinates;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Surface
{
    public class LaunchSiteTests
    {
        [Fact]
        public void Create()
        {
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite(33,"l1", th.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0), Astrodynamics.Tests.Constants.SitePath, new AzimuthRange(1.0, 2.0));
            Assert.Equal("l1", site.Name);
            Assert.Equal(th.GetEarthAtJ2000(), site.Body);
            Assert.Equal(new Geodetic(1.0, 2.0, 3.0), site.Geodetic);
            Assert.Single(site.AzimuthRanges);
            Assert.Equal(new AzimuthRange(1.0, 2.0), site.AzimuthRanges.First());
        }


        [Fact]
        public void IsAzimuthAllowed()
        {
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite(33,"l1", th.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0), Astrodynamics.Tests.Constants.SitePath,new AzimuthRange(1.0, 2.0), new AzimuthRange(4.0, 5.0));
            Assert.True(site.IsAzimuthAllowed(1.0));
            Assert.True(site.IsAzimuthAllowed(5.0));
            Assert.False(site.IsAzimuthAllowed(3.0));
            Assert.False(site.IsAzimuthAllowed(-1.0));
        }
    }
}
