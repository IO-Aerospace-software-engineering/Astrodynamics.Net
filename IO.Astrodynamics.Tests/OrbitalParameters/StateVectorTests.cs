using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;

namespace IO.Astrodynamics.Tests.OrbitalParameters
{
    public class StateVectorTests
    {
        public StateVectorTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }
        [Fact]
        public void Create()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);

            Vector3 pos = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel = new Vector3(4.0, 5.0, 6.0);
            var epoch = new DateTime(2021, 12, 12);
            StateVector sv = new StateVector(pos, vel, earth, epoch, Frames.Frame.ICRF);
            Assert.Equal(earth, sv.CenterOfMotion);
            Assert.Equal(epoch, sv.Epoch);
            Assert.Equal(Frames.Frame.ICRF, sv.Frame);
            Assert.Equal(pos, sv.Position);
            Assert.Equal(vel, sv.Velocity);
        }

        [Fact]
        public void Eccentricity()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(-6.116559469556896E+03, -1.546174698676721E+03, 2.521950157430313E+03), new Vector3(-8.078523150700097E-01, -5.477647950892673, -5.297615757935174), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(1.3532064672852535E-03, sv.Eccentricity());
        }

        [Fact]
        public void EccentricityVector()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 8.0, 0.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Vector3 ev = sv.EccentricityVector();
            Assert.Equal(0.09182016466094156, ev.Magnitude());
            Assert.Equal(0.09182016466094156, ev.X);
            Assert.Equal(0.0, ev.Y);
            Assert.Equal(0.0, ev.Z);
        }

        [Fact]
        public void SpecificAngularMomentum()
        {
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            
            StateVector sv = new StateVector(new Vector3(149600000.0, 0.0, 0.0), new Vector3(0.0, 29.8, 0.0), sun, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(4458080000.0, sv.SpecificAngularMomentum().Magnitude());
            Assert.Equal(0.0, sv.SpecificAngularMomentum().X);
            Assert.Equal(0.0, sv.SpecificAngularMomentum().Y);
            Assert.Equal(4458080000.0, sv.SpecificAngularMomentum().Z);
        }

        [Fact]
        public void SpecificOrbitalEnergyMomentum()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(-6.116559469556896E+03, -1.546174698676721E+03, 2.521950157430313E+03), new Vector3(-8.078523150700097E-01, -5.477647950892673, -5.297615757935174), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(-29.305466524713985, sv.SpecificOrbitalEnergy()); //ISS orbital energy in MJ
        }

        [Fact]
        public void Inclination()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 5.0, 5.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(System.Math.PI / 4.0, sv.Inclination());
        }

        [Fact]
        public void SemiMajorAxis()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(8000.0, 0.0, 0.0), new Vector3(0.0, 6.0, 6.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(14415.871593742577, sv.SemiMajorAxis());
        }

        [Fact]
        public void AscendingNodeVector()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(8000.0, 0.0, 0.0), new Vector3(0.0, 6.0, 6.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            var v = sv.AscendingNodeVector().Normalize();
            Assert.Equal(1.0, v.X);
            Assert.Equal(0.0, v.Y);
            Assert.Equal(0.0, v.Z);
        }

        [Fact]
        public void AscendingNode()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(9208.0, 3352, 0.0), new Vector3(-1.75, 4.83, 5.14), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(20.0030883092998, sv.AscendingNode() *IO.Astrodynamics.Constants.Rad2Deg);

            sv = new StateVector(new Vector3(7507.0, -6299, 0.0), new Vector3(3.3, 3.93, 5.14), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(320.0005416342622, sv.AscendingNode() *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void ArgumentOfPeriapis()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(8237.0, 17.0, 5308.0), new Vector3(-2.0, 6.0, 3.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(53.62182047159395, sv.ArgumentOfPeriapsis() *IO.Astrodynamics.Constants.Rad2Deg);

            sv = new StateVector(new Vector3(3973.0, -4881.0, -931.0), new Vector3(5.4, 3.36, 5.89), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(350.5106160942597, sv.ArgumentOfPeriapsis() *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void TrueAnomaly()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(5070.0, -2387.0, 1430.0), new Vector3(2.45, 6.35, 6.44), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(30.575383721029773, sv.TrueAnomaly() *IO.Astrodynamics.Constants.Rad2Deg);

            sv = new StateVector(new Vector3(1664.0, -4862.0, -2655.0), new Vector3(7.52, 0.89, 5.52), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(329.4769992556316, sv.TrueAnomaly() *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void EccentricAnomaly()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(6700.0, 2494.0, 0.0), new Vector3(-2.15, 8.85, 0.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(0.2077617460590267, sv.EccentricAnomaly());
        }

        [Fact]
        public void MeanAnomaly()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(-5775.068936894231, -3372.353197848874, 651.695854037289), new Vector3(-0.661469579672604, -7.147573777688288, -2.915719736461653), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(59.99823682142395, sv.MeanAnomaly() *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void Period()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(-5775.068936894231, -3372.353197848874, 651.695854037289), new Vector3(-0.661469579672604, -7.147573777688288, -2.915719736461653), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(1.5501796250555555, sv.Period().TotalHours);
        }

        [Fact]
        public void MeanMotion()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            StateVector sv = new StateVector(new Vector3(-5775.068936894231, -3372.353197848874, 651.695854037289), new Vector3(-0.661469579672604, -7.147573777688288, -2.915719736461653), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(0.0011258883962765153, sv.MeanMotion());
        }

        [Fact]
        public void ToFrame()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);

            //J2000->Ecliptic
            //Earth from sun at 0 TDB
            var sv = new StateVector(new Vector3(-26499033.67742509, 132757417.33833946, 57556718.47053819), new Vector3(-29.79426007, -5.01805231, -2.17539380), earth, new DateTime(2000, 1, 1, 12, 0, 0), Frames.Frame.ICRF);

            double[] res = sv.ToFrame(Frames.Frame.ECLIPTIC).ToStateVector().ToArray();
            Assert.Equal(-26499033.67742509, res[0]);
            Assert.Equal(144697296.7925432, res[1]);
            Assert.Equal(-611.1494260467589, res[2]);
            Assert.Equal(-29.79426007, res[3]);
            Assert.Equal(-5.46929493974574, res[4]);
            Assert.Equal(0.0001817867528557393, res[5]);
        }

        [Fact]
        public void ToNonInertialFrame()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            var earthFrame = new Frames.Frame(PlanetsAndMoons.EARTH.Frame);

            //J2000->IAU_EARTH
            //Earth from sun at 0 TDB
            var sv = new StateVector(new Vector3(-26499033.67742509, 132757417.33833946, 57556718.47053819), new Vector3(-29.79426007, -5.01805231, -2.17539380), earth, epoch.ToTDB(), Frames.Frame.ICRF);

            double[] res = sv.ToFrame(earthFrame).ToStateVector().ToArray();
            Assert.Equal(-135352868.83029744, res[0]);
            Assert.Equal(-2583535.869143948, res[1]);
            Assert.Equal(57553737.733541526, res[2]);
            Assert.Equal(-188.61117766406102, res[3]);
            Assert.Equal(9839.761815065332, res[4]);
            Assert.Equal(-1.9036033768843523, res[5]);
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