using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Surface
{
    public class SiteTests
    {
        public SiteTests()
        {
            ;
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }

        [Fact]
        public void StateVector()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);
            Site site = new Site(13, "DSS-13", earth);

            var sv = site.GetEphemeris(epoch, earth, Frames.Frame.ICRF, Aberration.None);

            Assert.Equal(
                new StateVector(new Vector3(-4998233.546875491, 1489959.5686882124, 3660827.795151215),
                    new Vector3(-108.65703034589836, -364.46975238850325, -0.013117008722929138), earth, epoch,
                    Frames.Frame.ICRF), sv);
        }


        [Fact]
        public void GetHorizontalCoordinates()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);

            Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000);
            var hor = site.GetHorizontalCoordinates(epoch, TestHelpers.MoonAtJ2000, Aberration.None);
            Assert.Equal(117.89631806108865, hor.Azimuth * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(16.79061677201462, hor.Elevation * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(400552679.30743355, hor.Range);
        }

        [Fact]
        public void GetHorizontalCoordinates2()
        {
            var epoch = new DateTime(2000, 1, 5, 12, 0, 0);

            Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000);
            var hor = site.GetHorizontalCoordinates(epoch, TestHelpers.MoonAtJ2000, Aberration.None);
            Assert.Equal(100.01881371927551, hor.Azimuth * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(-23.23601238553318, hor.Elevation * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(408535095.85180473, hor.Range);
        }

        [Fact]
        public void GetHorizontalCoordinates3()
        {
            var epoch = new DateTime(2000, 1, 10, 12, 0, 0);

            Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000);
            var hor = site.GetHorizontalCoordinates(epoch, TestHelpers.MoonAtJ2000, Aberration.None);
            Assert.Equal(41.60830471508871, hor.Azimuth * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(-63.02074114148227, hor.Elevation * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(401248015.68680006, hor.Range);
        }

        [Fact]
        public void GetHorizontalCoordinates4()
        {
            var epoch = new DateTime(2000, 1, 15, 12, 0, 0);

            Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000);
            var hor = site.GetHorizontalCoordinates(epoch, TestHelpers.MoonAtJ2000, Aberration.None);
            Assert.Equal(312.5426255803723, hor.Azimuth * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(-33.618934779034475, hor.Elevation * IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(376638211.1106281, hor.Range);
        }

        [Fact]
        public void Create()
        {
            var epoch = new DateTime(2000, 1, 15, 12, 0, 0);

            Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000);
            Assert.Equal(13, site.Id);
            Assert.Equal(TestHelpers.EarthAtJ2000.NaifId * 1000 + site.Id, site.NaifId);
            Assert.Equal("DSS-13", site.Name);
            Assert.Equal(TestHelpers.EarthAtJ2000, site.Body);
        }

        [Fact]
        public void CreateException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Site(-13, "DSS-13", TestHelpers.EarthAtJ2000));
            Assert.Throws<ArgumentException>(() => new Site(13, "", TestHelpers.EarthAtJ2000));
            Assert.Throws<ArgumentNullException>(() => new Site(13, "DSS-13", null));
        }

        [Fact]
        public void GetEphemeris()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);
            Site site = new Site(13, "DSS-13", earth);

            var sv = site.GetEphemeris(new Window(epoch, epoch + TimeSpan.FromDays(1.0)), earth, Frames.Frame.ICRF, Aberration.None, TimeSpan.FromDays(1.0)).ToArray();

            Assert.Equal(2, sv.Length);
            Assert.Equal(new StateVector(new Vector3(-4998233.546875491, 1489959.5686882124, 3660827.795151215),
                new Vector3(-108.65703034589836, -364.46975238850325, -0.013117008722929138), earth, epoch, Frames.Frame.ICRF), sv[0]);
            Assert.Equal(new StateVector(new Vector3(-5023123.881176386, 1403764.4798262327, 3660826.410895818),
                new Vector3(-102.37160967215728, -366.2848678619536, -0.012999877880587764), earth, epoch + TimeSpan.FromDays(1.0), Frames.Frame.ICRF), sv[1]);
        }

        [Fact]
        public void GetPosition()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);
            Site site = new Site(13, "DSS-13", earth);

            var sv = site.GetPosition(epoch, earth, Frames.Frame.ICRF, Aberration.None);

            Assert.Equal(new Vector3(4998233.546875491, -1489959.5686882124, -3660827.795151215), sv);
        }

        [Fact]
        public void GetVelocity()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);
            Site site = new Site(13, "DSS-13", earth);

            var sv = site.GetVelocity(epoch, earth, Frames.Frame.ICRF, Aberration.None);

            Assert.Equal(new Vector3(108.65703034589836, 364.46975238850325, 0.013117008722929138), sv);
        }

        [Fact]
        public void GetAngularSepartion()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            CelestialBody moon = new CelestialBody(PlanetsAndMoons.MOON.NaifId);
            Site site = new Site(13, "DSS-13", moon.InitialOrbitalParameters.CenterOfMotion);

            var separation = site.AngularSeparation(epoch, moon, site.Body.InitialOrbitalParameters.CenterOfMotion, Aberration.None);

            Assert.Equal(0.9844974681377541, separation);
        }

        [Fact]
        public void FindWindowsOnDistanceConstraint()
        {
            CelestialBody moon = new CelestialBody(PlanetsAndMoons.MOON.NaifId);
            Site site = new Site(13, "DSS-13", moon.InitialOrbitalParameters.CenterOfMotion);

            var res = site.FindWindowsOnDistanceConstraint(new Window(DateTimeExtension.CreateTDB(220881665.18391809), DateTimeExtension.CreateTDB(228657665.18565452)),
                TestHelpers.MoonAtJ2000, RelationnalOperator.Greater, 400000000, Aberration.None, TimeSpan.FromSeconds(86400.0));
            var windows = res as Window[] ?? res.ToArray();
            Assert.Equal(2, windows.Count());
            Assert.Equal("2007-02-03T17:02:04.4460000 (TDB)", windows.ElementAt(0).StartDate.ToFormattedString());
            Assert.Equal("2007-02-09T10:31:41.4310000 (TDB)", windows.ElementAt(0).EndDate.ToFormattedString());
            Assert.Equal("2007-03-30T11:09:38.6990000 (TDB)", windows.ElementAt(1).StartDate.ToFormattedString());
            Assert.Equal("2007-04-01T00:01:05.1860000 (TDB)", windows.ElementAt(1).EndDate.ToFormattedString());
        }

        [Fact]
        public void FindWindowsOnOccultationConstraint()
        {
            CelestialBody moon = new CelestialBody(PlanetsAndMoons.MOON.NaifId);
            Site site = new Site(13, "DSS-13", moon.InitialOrbitalParameters.CenterOfMotion);

            var res = site.FindWindowsOnOccultationConstraint(new Window(DateTime.Parse("2005-10-03 00:00:00"), DateTime.Parse("2005-10-04 00:00:00")),
                TestHelpers.Sun, ShapeType.Ellipsoid, TestHelpers.MoonAtJ2000, ShapeType.Ellipsoid, OccultationType.Partial, Aberration.None, TimeSpan.FromSeconds(360.0));
            var windows = res as Window[] ?? res.ToArray();
            Assert.Single(windows);
            Assert.Equal("2005-10-03T08:37:48.4010000 (TDB)", windows.ElementAt(0).StartDate.ToFormattedString());
            Assert.Equal("2005-10-03T10:15:20.0620000 (TDB)", windows.ElementAt(0).EndDate.ToFormattedString());
        }

        [Fact]
        public void FindWindowsOnCoordinateConstraint()
        {
            Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000);

            var res = site.FindWindowsOnCoordinateConstraint(new Window(DateTime.Parse("2005-10-03").ToTDB(), DateTime.Parse("2005-11-03").ToTDB()), TestHelpers.MoonAtJ2000,
                TestHelpers.MoonAtJ2000.Frame, CoordinateSystem.Latitudinal, Coordinate.Latitude, RelationnalOperator.Greater, 0.0, 0.0, Aberration.None,
                TimeSpan.FromSeconds(60.0));

            var windows = res as Window[] ?? res.ToArray();
            Assert.Equal(2, windows.Length);
            Assert.Equal("2005-10-03T13:52:57.9510000 (TDB)", windows[0].StartDate.ToFormattedString());
            Assert.Equal("2005-10-17T03:42:05.5380000 (TDB)", windows[0].EndDate.ToFormattedString());
            Assert.Equal("2005-10-30T16:50:14.1420000 (TDB)", windows[1].StartDate.ToFormattedString());
            Assert.Equal("2005-11-03T00:00:00.0000000 (TDB)", windows[1].EndDate.ToFormattedString());
        }

        [Fact]
        public void FindWindowsOnIlluminationConstraint()
        {
            Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000,
                new Geodetic(-116.79445837 * Astrodynamics.Constants.Deg2Rad, 35.24716450 * Astrodynamics.Constants.Deg2Rad, 1070.85059));

            var res = site.FindWindowsOnIlluminationConstraint(new Window(DateTimeExtension.CreateTDB(674524800), DateTimeExtension.CreateTDB(674611200)), TestHelpers.Sun,
                IlluminationAngle.Incidence, RelationnalOperator.Lower, System.Math.PI * 0.5 - (-0.8 * IO.Astrodynamics.Constants.Deg2Rad), 0.0, Aberration.CNS, TimeSpan.FromSeconds(3600), TestHelpers.Sun);

            var windows = res as Window[] ?? res.ToArray();
            Assert.Single(windows);
            Assert.Equal("2021-05-17T12:51:01.1100000 (TDB)", windows[0].StartDate.ToFormattedString());
            Assert.Equal("2021-05-18T02:55:45.3300000 (TDB)", windows[0].EndDate.ToFormattedString());
        }
    }
}