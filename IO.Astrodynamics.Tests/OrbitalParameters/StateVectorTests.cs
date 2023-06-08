using System;
using Xunit;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Tests.OrbitalParameters
{
    public class StateVectorTests
    {
        [Fact]
        public void Create()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            Vector3 pos = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel = new Vector3(4.0, 5.0, 6.0);
            var epoch = new DateTime(2021, 12, 12);
            StateVector sv = new StateVector(pos, vel, earthScn, epoch, Frames.Frame.ICRF);
            Assert.Equal(earthScn, sv.CenterOfMotion);
            Assert.Equal(epoch, sv.Epoch);
            Assert.Equal(Frames.Frame.ICRF, sv.Frame);
            Assert.Equal(pos, sv.Position);
            Assert.Equal(vel, sv.Velocity);
        }

        [Fact]
        public void Eccentricity()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(-6.116559469556896E+03, -1.546174698676721E+03, 2.521950157430313E+03), new Vector3(-8.078523150700097E-01, -5.477647950892673, -5.297615757935174), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(1.3532064672852535E-03, sv.Eccentricity());
        }

        [Fact]
        public void EccentricityVector()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 8.0, 0.0), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Vector3 ev = sv.EccentricityVector();
            Assert.Equal(0.09182016466094156, ev.Magnitude());
            Assert.Equal(0.09182016466094156, ev.X);
            Assert.Equal(0.0, ev.Y);
            Assert.Equal(0.0, ev.Z);
        }

        [Fact]
        public void SpecificAngularMomentum()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);
            StateVector sv = new StateVector(new Vector3(149600000.0, 0.0, 0.0), new Vector3(0.0, 29.8, 0.0), sunScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(4458080000.0, sv.SpecificAngularMomentum().Magnitude());
            Assert.Equal(0.0, sv.SpecificAngularMomentum().X);
            Assert.Equal(0.0, sv.SpecificAngularMomentum().Y);
            Assert.Equal(4458080000.0, sv.SpecificAngularMomentum().Z);
        }

        [Fact]
        public void SpecificOrbitalEnergyMomentum()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(-6.116559469556896E+03, -1.546174698676721E+03, 2.521950157430313E+03), new Vector3(-8.078523150700097E-01, -5.477647950892673, -5.297615757935174), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(-29.305466524713985, sv.SpecificOrbitalEnergy()); //ISS orbital energy in MJ
        }

        [Fact]
        public void Inclination()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 5.0, 5.0), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(System.Math.PI / 4.0, sv.Inclination());
        }

        [Fact]
        public void SemiMajorAxis()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(8000.0, 0.0, 0.0), new Vector3(0.0, 6.0, 6.0), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(14415.871593742577, sv.SemiMajorAxis());
        }

        [Fact]
        public void AscendingNodeVector()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(8000.0, 0.0, 0.0), new Vector3(0.0, 6.0, 6.0), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);
            var v = sv.AscendingNodeVector().Normalize();
            Assert.Equal(1.0, v.X);
            Assert.Equal(0.0, v.Y);
            Assert.Equal(0.0, v.Z);
        }

        [Fact]
        public void AscendingNode()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(9208.0, 3352, 0.0), new Vector3(-1.75, 4.83, 5.14), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(20.0030883092998, sv.AscendingNode() * Constants.Rad2Deg);

            sv = new StateVector(new Vector3(7507.0, -6299, 0.0), new Vector3(3.3, 3.93, 5.14), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(320.0005416342622, sv.AscendingNode() * Constants.Rad2Deg);
        }

        [Fact]
        public void ArgumentOfPeriapis()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(8237.0, 17.0, 5308.0), new Vector3(-2.0, 6.0, 3.0), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(53.62182047159395, sv.ArgumentOfPeriapsis() * Constants.Rad2Deg);

            sv = new StateVector(new Vector3(3973.0, -4881.0, -931.0), new Vector3(5.4, 3.36, 5.89), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(350.5106160942597, sv.ArgumentOfPeriapsis() * Constants.Rad2Deg);
        }

        [Fact]
        public void TrueAnomaly()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(5070.0, -2387.0, 1430.0), new Vector3(2.45, 6.35, 6.44), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(30.575383721029773, sv.TrueAnomaly() * Constants.Rad2Deg);

            sv = new StateVector(new Vector3(1664.0, -4862.0, -2655.0), new Vector3(7.52, 0.89, 5.52), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(329.4769992556316, sv.TrueAnomaly() * Constants.Rad2Deg);
        }

        [Fact]
        public void EccentricAnomaly()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(6700.0, 2494.0, 0.0), new Vector3(-2.15, 8.85, 0.0), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(0.2077617460590267, sv.EccentricAnomaly());
        }

        [Fact]
        public void MeanAnomaly()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(-5775.068936894231, -3372.353197848874, 651.695854037289), new Vector3(-0.661469579672604, -7.147573777688288, -2.915719736461653), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(59.99823682142395, sv.MeanAnomaly() * Constants.Rad2Deg);
        }

        [Fact]
        public void Period()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(-5775.068936894231, -3372.353197848874, 651.695854037289), new Vector3(-0.661469579672604, -7.147573777688288, -2.915719736461653), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(1.5501796250555555, sv.Period().TotalHours);
        }

        [Fact]
        public void MeanMotion()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            StateVector sv = new StateVector(new Vector3(-5775.068936894231, -3372.353197848874, 651.695854037289), new Vector3(-0.661469579672604, -7.147573777688288, -2.915719736461653), earthScn, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(0.0011258883962765153, sv.MeanMotion());
        }

        [Fact]
        public void ToFrame()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            //J2000->Ecliptic
            //Earth from sun at 0 TDB
            double[] earthSv = { -26499033.67742509, 132757417.33833946, 57556718.47053819, -29.79426007, -5.01805231, -2.17539380 };
            var sv = new StateVector(new Vector3(-26499033.67742509, 132757417.33833946, 57556718.47053819), new Vector3(-29.79426007, -5.01805231, -2.17539380), earthScn, new DateTime(2000, 1, 1, 12, 0, 0), Frames.Frame.ICRF);

            double[] res = sv.ToFrame(Frames.Frame.ECLIPTIC).ToStateVector().ToArray();
            Assert.Equal(-26499033.614230465, res[0]);
            Assert.Equal(144697296.44746894, res[1]);
            Assert.Equal(-611.521793436259, res[2]);
            Assert.Equal(-29.794259998946952, res[3]);
            Assert.Equal(-5.4692949267021529, res[4]);
            Assert.Equal(0.00018180082728846969, res[5]);
        }

        [Fact]
        public void ToNonInertialFrame()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            var earthFrame = new Frames.Frame("earth");

            //J2000->IAU_ERATH
            //Earth from sun at 0 TDB
            var sv = new StateVector(new Vector3(-26499033.67742509, 132757417.33833946, 57556718.47053819), new Vector3(-29.79426007, -5.01805231, -2.17539380), earthScn, new DateTime(2000, 1, 1, 12, 0, 0), Frames.Frame.ICRF);

            double[] res = sv.ToFrame(earthFrame).ToStateVector().ToArray();
            Assert.Equal(-135349405.82736558, res[0]);
            Assert.Equal(-2696123.4810504094, res[1]);
            Assert.Equal(57556718.6204426, res[2]);
            Assert.Equal(-196.91401917792356, res[3]);
            Assert.Equal(9839.6220521992618, res[4]);
            Assert.Equal(-2.1758107565986347, res[5]);
        }

        [Fact]
        public void PerigeeVelocity()
        {
            
            var earth = TestHelpers.GetEarthAtJ2000();
            var sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 8.0, 0.0), earth, earth.InitialOrbitalParameters.Epoch, Frames.Frame.ICRF);
            Assert.Equal(8.0, sv.PerigeeVelocity());
            
        }

        [Fact]
        public void ApogeeVelocity()
        {
            
            var earth = TestHelpers.GetEarthAtJ2000();
            var sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 8.0, 0.0), earth, earth.InitialOrbitalParameters.Epoch, Frames.Frame.ICRF);
            Assert.Equal(6.65442800735294, sv.ApogeeVelocity());

        }
    }
}