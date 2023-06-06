using IO.Astrodynamics.Models.Surface;
using System;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;
using Launch = IO.Astrodynamics.Models.Maneuver.Launch;
using Site = IO.Astrodynamics.Models.Surface.Site;

namespace IO.Astrodynamics.Models.Tests.Maneuvers
{
    public class LaunchTests
    {
        [Fact]
        public void CreateWithBody()
        {
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0), default, new AzimuthRange(1.0, 2.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0));
            Launch launch = new Launch(site, recoverySite, th.GetMoon(), Constants.CivilTwilight, true);
            Assert.NotNull(launch);
            Assert.Equal(site, launch.LaunchSite);
            Assert.Equal(recoverySite, launch.RecoverySite);
            Assert.True(launch.LaunchByDay);
            Assert.Equal(th.GetMoon(), launch.TargetBody);
            Assert.Null(launch.TargetOrbit);
        }

        [Fact]
        public void CreateWithOrbitalParameter()
        {
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0), default, new AzimuthRange(1.0, 2.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(1.0, 2.0, 3.0));
            Launch launch = new Launch(site, recoverySite, th.GetMoon().InitialOrbitalParameters, Constants.CivilTwilight, true);
            Assert.NotNull(launch);
            Assert.Equal(site, launch.LaunchSite);
            Assert.Equal(recoverySite, launch.RecoverySite);
            Assert.True(launch.LaunchByDay);
            Assert.Equal(th.GetMoon().InitialOrbitalParameters, launch.TargetOrbit);
            Assert.Null(launch.TargetBody);
        }

        [Fact]
        public void InertialAscendingAzimuth()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 51.6494 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetInertialAscendingAzimuthLaunch(epoch);
            Assert.Equal(44.912872404793241, res * Constants.Rad2Deg, 3);
        }

        [Fact]
        public void InertialDescendingAzimuth()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 51.6494 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetInertialDescendingAzimuthLaunch(epoch);
            Assert.Equal(180 - 44.912872404793241, res * Constants.Rad2Deg, 3);
        }

        [Fact]
        public void InertialInsertionVelocity()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 51.6494 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetInertialInsertionVelocity(epoch);
            Assert.Equal(7.6969997304533663, res, 3);
        }

        [Fact]
        public void NonInertialAscendingAzimuth()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 51.6494 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetNonInertialAscendingAzimuthLaunch(epoch);
            Assert.Equal(42.675344642468474, res * Constants.Rad2Deg, 3);
        }

        [Fact]
        public void NonInertialDescendingAzimuth()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 51.6494 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetNonInertialDescendingAzimuthLaunch(epoch);
            Assert.Equal(180 - 42.675344642468474, res * Constants.Rad2Deg, 3);
        }

        [Fact]
        public void NonInertialInsertionVelocity()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 51.6494 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetNonInertialInsertionVelocity(epoch);
            Assert.Equal(7.4138589742455188, res, 3);
        }

        [Fact]
        public void RetrogradInertialAscendingAzimuth()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 110.0 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetInertialAscendingAzimuthLaunch(epoch);
            Assert.Equal(337.09636518334463, res * Constants.Rad2Deg, 3);
        }

        [Fact]
        public void RetrogradNonInertialAscendingAzimuth()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 110.0 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetNonInertialAscendingAzimuthLaunch(epoch);
            Assert.Equal(334.352, res * Constants.Rad2Deg, 3);
        }

        [Fact]
        public void RetrogradNonInertialInsertionVelocity()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 140.0 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetNonInertialInsertionVelocity(epoch);
            Assert.Equal(8.056, res, 3);
        }

        [Fact]
        public void RetrogradInertialInsertionVelocity()
        {
            var epoch = new DateTime(2013, 10, 14, 10, 18, 0);
            var th = new TestHelpers();
            LaunchSite site = new LaunchSite("l1", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", th.GetEarthAtJ2000(), new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            Launch launch = new Launch(site, recoverySite, new KeplerianElements(6728.137, 0.0, 140.0 * Constants.Deg2Rad, 0.0, 0.0, 0.0, th.GetEarthAtJ2000(), epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.GetInertialInsertionVelocity(epoch);
            Assert.Equal(7.6969997304533663, res, 3);
        }

        [Fact]
        public void FindLaunchWindows()
        {
            var epoch = new DateTime(2021, 6, 2);
            var th = new TestHelpers();
            var earth = th.GetEarth();
            earth.AddStateOrientationFromICRF(new StateOrientation(new Quaternion(0.57445446, 0.00084188, -0.00058582, 0.81853590), new Vector3(1.51823816E-07, -3.60693290E-10, 7.29209921E-05), epoch, Frames.Frame.ICRF));
            LaunchSite site = new LaunchSite("l1", earth, new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", earth, new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite, new StateVector(new Vector3(-1.144243683349977E+03, 4.905086030671815E+03, 4.553416875193589E+03), new Vector3(-5.588288926819989E+00, -4.213222250603758E+00, 3.126518392859475E+00), earth, epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)));
            Assert.Equal(2, res.Count());
            var firstWindow = res[0];
            Assert.Equal(Convert.ToDateTime("2021-06-02 02:52:03.4942137"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02 02:52:03.4942137"), firstWindow.Window.EndDate);
            Assert.Equal(135.1619859153713, firstWindow.InertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(137.41309272638028, firstWindow.NonInertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(7.6670268762217848, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7.3843116897175163, firstWindow.NonInertialInsertionVelocity, 6);

            var secondWindow = res[1];
            Assert.Equal(Convert.ToDateTime("2021-06-02 18:13:32.4599782"), secondWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02 18:13:32.4599782"), secondWindow.Window.EndDate);
            Assert.Equal(45.106102519620528, secondWindow.InertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(42.8697243790726, secondWindow.NonInertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(7.6670268762217848, secondWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7.3834621503134024, secondWindow.NonInertialInsertionVelocity, 6);
        }

        [Fact]
        public void FindLaunchWindowsByDay()
        {
            var epoch = new DateTime(2021, 6, 2);
            var th = new TestHelpers();
            var earth = th.GetEarth();
            earth.AddStateOrientationFromICRF(new StateOrientation(new IO.Astrodynamics.Models.Math.Quaternion(0.57445446, 0.00084188, -0.00058582, 0.81853590), new Vector3(1.51823816E-07, -3.60693290E-10, 7.29209921E-05), epoch, Frames.Frame.ICRF));
            LaunchSite site = new LaunchSite("l1", earth, new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", earth, new Geodetic(-81.0 * Constants.Deg2Rad, 28.5 * Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite, new StateVector(new Vector3(-1.144243683349977E+03, 4.905086030671815E+03, 4.553416875193589E+03), new Vector3(-5.588288926819989E+00, -4.213222250603758E+00, 3.126518392859475E+00), earth, epoch, Frames.Frame.ICRF), Constants.CivilTwilight, true);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)));
            Assert.Single(res);
            var firstWindow = res[0];
            Assert.Equal(Convert.ToDateTime("2021-06-02T18:13:31.7747382"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02T18:13:31.7747382"), firstWindow.Window.EndDate);
            Assert.Equal(45.106104650222136, firstWindow.InertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(42.8697266220075, firstWindow.NonInertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(7.6670268762217848, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7.3834621430387886, firstWindow.NonInertialInsertionVelocity, 6);
        }

        [Fact]
        public void FindSouthLaunchWindowsByDay()
        {
            var epoch = new DateTime(2021, 6, 2);
            var th = new TestHelpers();
            var earth = th.GetEarth();
            earth.AddStateOrientationFromICRF(new StateOrientation(new IO.Astrodynamics.Models.Math.Quaternion(0.57445446, 0.00084188, -0.00058582, 0.81853590), new Vector3(1.51823816E-07, -3.60693290E-10, 7.29209921E-05), epoch, Frames.Frame.ICRF));
            LaunchSite site = new LaunchSite("l1", earth, new Geodetic(-104.0 * Constants.Deg2Rad, -41.0 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", earth, new Geodetic(-104.0 * Constants.Deg2Rad, -41.0 * Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite, new StateVector(new Vector3(-1.144243683349977E+03, 4.905086030671815E+03, 4.553416875193589E+03), new Vector3(-5.588288926819989E+00, -4.213222250603758E+00, 3.126518392859475E+00), earth, epoch, Frames.Frame.ICRF), Constants.CivilTwilight, true);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)));
            Assert.Single(res);
            var firstWindow = res[0];
            Assert.Equal(Convert.ToDateTime("2021-06-02 15:09:02.8116989"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02 15:09:02.8116989"), firstWindow.Window.EndDate);
            Assert.Equal(55.559276498467867, firstWindow.InertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(54.016918589294363, firstWindow.NonInertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(7.6670268762217848, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7.3800349363081423, firstWindow.NonInertialInsertionVelocity, 6);
        }

        [Fact]
        public void FindSouthLaunchWindows()
        {
            var epoch = new DateTime(2021, 6, 2);
            var th = new TestHelpers();
            var earth = th.GetEarth();
            earth.AddStateOrientationFromICRF(new StateOrientation(new IO.Astrodynamics.Models.Math.Quaternion(0.57445446, 0.00084188, -0.00058582, 0.81853590), new Vector3(1.51823816E-07, -3.60693290E-10, 7.29209921E-05), epoch, Frames.Frame.ICRF));
            LaunchSite site = new LaunchSite("l1", earth, new Geodetic(-104.0 * Constants.Deg2Rad, -41.0 * Constants.Deg2Rad, 0.0), default, new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site("l2", earth, new Geodetic(-104.0 * Constants.Deg2Rad, -41.0 * Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite, new StateVector(new Vector3(-1.144243683349977E+03, 4.905086030671815E+03, 4.553416875193589E+03), new Vector3(-5.588288926819989E+00, -4.213222250603758E+00, 3.126518392859475E+00), earth, epoch, Frames.Frame.ICRF), Constants.CivilTwilight, null);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)));
            Assert.Equal(2, res.Count());
            var firstWindow = res[0];
            Assert.Equal(Convert.ToDateTime("2021-06-02 08:58:43.3705101"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02 08:58:43.3705101"), firstWindow.Window.EndDate);
            Assert.Equal(124.74777778701655, firstWindow.InertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(126.29778811432655, firstWindow.NonInertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(7.6670268762217848, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7.38187255762301, firstWindow.NonInertialInsertionVelocity, 6);

            var secondWindow = res[1];
            Assert.Equal(Convert.ToDateTime("2021-06-02 15:09:03.4969388"), secondWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02 15:09:03.4969388"), secondWindow.Window.EndDate);
            Assert.Equal(55.559281941132859, secondWindow.InertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(54.016924123319619, secondWindow.NonInertialAzimuth * Constants.Rad2Deg, 6);
            Assert.Equal(7.6670268762217848, secondWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7.38003489572904, secondWindow.NonInertialInsertionVelocity, 6);
        }
    }
}
