using System;
using System.Linq;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers
{
    public class LaunchTests
    {
        public LaunchTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }

        [Fact]
        public void CreateWithBody()
        {
            LaunchSite site = new LaunchSite(33, "l1", TestHelpers.EarthAtJ2000, new Planetocentric(1.0, 2.0, 3.0), default, new AzimuthRange(1.0, 2.0));
            Site recoverySite = new Site(34, "l2", TestHelpers.EarthAtJ2000, new Planetocentric(1.0, 2.0, 3.0));
            Launch launch = new Launch(site, recoverySite, TestHelpers.Moon, IO.Astrodynamics.Constants.CivilTwilight, true);
            Assert.NotNull(launch);
            Assert.Equal(site, launch.LaunchSite);
            Assert.Equal(recoverySite, launch.RecoverySite);
            Assert.True(launch.LaunchByDay);
            Assert.Equal(TestHelpers.Moon, launch.TargetBody);
            Assert.Null(launch.TargetOrbit);
        }

        [Fact]
        public void CreateWithOrbitalParameter()
        {
            LaunchSite site = new LaunchSite(33, "l1", TestHelpers.EarthAtJ2000, new Planetocentric(1.0, 2.0, 3.0), default, new AzimuthRange(1.0, 2.0));
            Site recoverySite = new Site(34, "l2", TestHelpers.EarthAtJ2000, new Planetocentric(1.0, 2.0, 3.0));
            Launch launch = new Launch(site, recoverySite, TestHelpers.Moon.InitialOrbitalParameters, IO.Astrodynamics.Constants.CivilTwilight, true);
            Assert.NotNull(launch);
            Assert.Equal(site, launch.LaunchSite);
            Assert.Equal(recoverySite, launch.RecoverySite);
            Assert.True(launch.LaunchByDay);
            Assert.Equal(TestHelpers.Moon.InitialOrbitalParameters, launch.TargetOrbit);
            Assert.Null(launch.TargetBody);
        }

        [Fact]
        public void FindLaunchWindows()
        {
            var epoch = new DateTime(2021, 6, 2);

            var earth = TestHelpers.Earth;
            LaunchSite site = new LaunchSite(33, "l1", earth, new Planetocentric(-81.0 * IO.Astrodynamics.Constants.Deg2Rad, 28.5 * IO.Astrodynamics.Constants.Deg2Rad, 0.0), default,
                new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site(34, "l2", earth, new Planetocentric(-81.0 * IO.Astrodynamics.Constants.Deg2Rad, 28.5 * IO.Astrodynamics.Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite,
                new StateVector(new Vector3(-1.144243683349977E+06, 4.905086030671815E+06, 4.553416875193589E+06),
                    new Vector3(-5.588288926819989E+03, -4.213222250603758E+03, 3.126518392859475E+03), earth, epoch, Frames.Frame.ICRF), IO.Astrodynamics.Constants.CivilTwilight,
                false);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)), Constants.OutputPath);
            var launchWindows = res as LaunchWindow[] ?? res.ToArray();
            Assert.Equal(2, launchWindows.Count());
            var firstWindow = launchWindows.ElementAt(0);
            Assert.Equal(Convert.ToDateTime("2021-06-02T02:51:27.8460000"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02T02:51:27.8460000"), firstWindow.Window.EndDate);
            Assert.Equal(135.195039, firstWindow.InertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(137.447464, firstWindow.NonInertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(7667.02685, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7384.476096, firstWindow.NonInertialInsertionVelocity, 6);

            var secondWindow = launchWindows.ElementAt(1);
            Assert.Equal(Convert.ToDateTime("2021-06-02T18:11:48.1520000"), secondWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02T18:11:48.1520000"), secondWindow.Window.EndDate);
            Assert.Equal(44.804961, secondWindow.InertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(42.552536, secondWindow.NonInertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(7667.02685, secondWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7384.476096, secondWindow.NonInertialInsertionVelocity, 6);
        }

        [Fact]
        public void FindLaunchWindowsByDay()
        {
            var epoch = new DateTime(2021, 6, 2);

            var earth = TestHelpers.Earth;
            LaunchSite site = new LaunchSite(33, "l1", earth, new Planetocentric(-81.0 * IO.Astrodynamics.Constants.Deg2Rad, 28.5 * IO.Astrodynamics.Constants.Deg2Rad, 0.0), default,
                new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site(34, "l2", earth, new Planetocentric(-81.0 * IO.Astrodynamics.Constants.Deg2Rad, 28.5 * IO.Astrodynamics.Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite,
                new StateVector(new Vector3(-1.144243683349977E+06, 4.905086030671815E+06, 4.553416875193589E+06),
                    new Vector3(-5.588288926819989E+03, -4.213222250603758E+03, 3.126518392859475E+03), earth, epoch, Frames.Frame.ICRF), IO.Astrodynamics.Constants.CivilTwilight,
                true);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)), Constants.OutputPath);
            Assert.Single(res);
            var firstWindow = res.ElementAt(0);
            Assert.Equal(Convert.ToDateTime("2021-06-02T18:11:08.6170000"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02T18:11:08.6170000"), firstWindow.Window.EndDate);
            Assert.Equal(44.804961, firstWindow.InertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(42.552536, firstWindow.NonInertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(7667.02685, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7384.476096, firstWindow.NonInertialInsertionVelocity, 6);
        }

        [Fact]
        public void FindSouthLaunchWindowsByDay()
        {
            var epoch = new DateTime(2021, 6, 2);

            var earth = TestHelpers.Earth;
            LaunchSite site = new LaunchSite(33, "l1", earth, new Planetocentric(-104.0 * IO.Astrodynamics.Constants.Deg2Rad, -41.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0), default,
                new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site(34, "l2", earth, new Planetocentric(-104.0 * IO.Astrodynamics.Constants.Deg2Rad, -41.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite,
                new StateVector(new Vector3(-1.144243683349977E+06, 4.905086030671815E+06, 4.553416875193589E+06),
                    new Vector3(-5.588288926819989E+03, -4.213222250603758E+03, 3.126518392859475E+03), earth, epoch, Frames.Frame.ICRF), IO.Astrodynamics.Constants.CivilTwilight,
                true);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)), Constants.OutputPath);
            Assert.Single(res);
            var firstWindow = res.ElementAt(0);
            Assert.Equal(Convert.ToDateTime("2021-06-02T15:10:04.2760000"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02T15:10:04.2760000"), firstWindow.Window.EndDate);
            Assert.Equal(55.142764, firstWindow.InertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(53.583073, firstWindow.NonInertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(7667.02685, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7381.309245, firstWindow.NonInertialInsertionVelocity, 6);
        }

        [Fact]
        public void FindSouthLaunchWindows()
        {
            var epoch = new DateTime(2021, 6, 2);

            var earth = TestHelpers.Earth;
            LaunchSite site = new LaunchSite(33, "l1", earth, new Planetocentric(-104.0 * IO.Astrodynamics.Constants.Deg2Rad, -41.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0),
                new AzimuthRange(0.0, 6.0));
            Site recoverySite = new Site(34, "l2", earth, new Planetocentric(-104.0 * IO.Astrodynamics.Constants.Deg2Rad, -41.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0));
            //ISS at 2021-06-02 TDB
            Launch launch = new Launch(site, recoverySite,
                new StateVector(new Vector3(-1.144243683349977E+06, 4.905086030671815E+06, 4.553416875193589E+06),
                    new Vector3(-5.588288926819989E+03, -4.213222250603758E+03, 3.126518392859475E+03), earth, epoch, Frames.Frame.ICRF), IO.Astrodynamics.Constants.CivilTwilight,
                null);
            var res = launch.FindLaunchWindows(new Window(epoch, TimeSpan.FromDays(1.0)), Constants.OutputPath);
            Assert.Equal(2, res.Count());
            var firstWindow = res.ElementAt(0);
            Assert.Equal(Convert.ToDateTime("2021-06-02T08:56:02.0650000"), firstWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02T08:56:02.0650000"), firstWindow.Window.EndDate);
            Assert.Equal(124.857236, firstWindow.InertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(126.416927, firstWindow.NonInertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(7667.02685, firstWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7381.309245, firstWindow.NonInertialInsertionVelocity, 6);

            var secondWindow = res.ElementAt(1);
            Assert.Equal(Convert.ToDateTime("2021-06-02T15:08:42.9120000"), secondWindow.Window.StartDate);
            Assert.Equal(Convert.ToDateTime("2021-06-02T15:08:42.9120000"), secondWindow.Window.EndDate);
            Assert.Equal(55.142764, secondWindow.InertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(53.583073, secondWindow.NonInertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg, 6);
            Assert.Equal(7667.02685, secondWindow.InertialInsertionVelocity, 6);
            Assert.Equal(7381.309245, secondWindow.NonInertialInsertionVelocity, 6);
        }
    }
}