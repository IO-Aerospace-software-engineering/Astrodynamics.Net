using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
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
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

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
        public void Inverse()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            Vector3 pos = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel = new Vector3(4.0, 5.0, 6.0);
            var epoch = new DateTime(2021, 12, 12);
            StateVector sv = new StateVector(pos, vel, earth, epoch, Frames.Frame.ICRF);
            var res = sv.Inverse();
            Assert.Equal(new StateVector(pos.Inverse(), vel.Inverse(), earth, epoch, Frames.Frame.ICRF), res);
        }

        [Fact]
        public void Add()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            Vector3 pos = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel = new Vector3(4.0, 5.0, 6.0);
            var epoch = new DateTime(2021, 12, 12);
            StateVector sv = new StateVector(pos, vel, earth, epoch, Frames.Frame.ICRF);

            Vector3 pos2 = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel2 = new Vector3(4.0, 5.0, 6.0);
            StateVector sv2 = new StateVector(pos2, vel2, earth, epoch, Frames.Frame.ICRF);
            var sv3 = sv + sv2;
            Assert.Equal(new StateVector(new Vector3(2.0, 4.0, 6.0), new Vector3(8.0, 10.0, 12.0), earth, epoch, Frames.Frame.ICRF), sv3);
        }

        [Fact]
        public void AddExcept()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            Vector3 pos = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel = new Vector3(4.0, 5.0, 6.0);
            var epoch = new DateTime(2021, 12, 12);
            StateVector sv = new StateVector(pos, vel, earth, epoch, Frames.Frame.ICRF);

            Vector3 pos2 = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel2 = new Vector3(4.0, 5.0, 6.0);
            StateVector sv2 = new StateVector(pos2, vel2, earth, epoch.Add(TimeSpan.FromSeconds(1.0)), Frames.Frame.ICRF);
            Assert.Throws<ArgumentException>(() => sv + sv2);
        }

        [Fact]
        public void Subtract()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            Vector3 pos = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel = new Vector3(4.0, 5.0, 6.0);
            var epoch = new DateTime(2021, 12, 12);
            StateVector sv = new StateVector(pos, vel, earth, epoch, Frames.Frame.ICRF);
            var sv2 = new StateVector(new Vector3(2.0, 4.0, 6.0), new Vector3(8.0, 10.0, 12.0), earth, epoch, Frames.Frame.ICRF);
            var sv3 = sv2 - sv;
            Assert.Equal(new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(4.0, 5.0, 6.0), earth, epoch, Frames.Frame.ICRF), sv3);
        }
        
        [Fact]
        public void SubtractExcept()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            Vector3 pos = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel = new Vector3(4.0, 5.0, 6.0);
            var epoch = new DateTime(2021, 12, 12);
            StateVector sv = new StateVector(pos, vel, earth, epoch, Frames.Frame.ICRF);

            Vector3 pos2 = new Vector3(1.0, 2.0, 3.0);
            Vector3 vel2 = new Vector3(4.0, 5.0, 6.0);
            StateVector sv2 = new StateVector(pos2, vel2, earth, epoch.Add(TimeSpan.FromSeconds(1.0)), Frames.Frame.ICRF);
            Assert.Throws<ArgumentException>(() => sv - sv2);
        }

        [Fact]
        public void Eccentricity()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(-6.116559469556896E+06, -1.546174698676721E+06, 2.521950157430313E+06),
                new Vector3(-8.078523150700097E+02, -5.477647950892673E+03, -5.297615757935174E+03), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(1.3532176446914895E-03, sv.Eccentricity());
        }

        [Fact]
        public void EccentricityVector()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 8000.0, 0.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Vector3 ev = sv.EccentricityVector();
            Assert.Equal(0.09182018209252996, ev.Magnitude());
            Assert.Equal(0.09182018209252996, ev.X);
            Assert.Equal(0.0, ev.Y);
            Assert.Equal(0.0, ev.Z);
        }

        [Fact]
        public void SpecificAngularMomentum()
        {
            CelestialBody sun = new CelestialBody(Stars.Sun.NaifId);

            StateVector sv = new StateVector(new Vector3(149600000.0, 0.0, 0.0), new Vector3(0.0, 29.8, 0.0), sun, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(4458080000.0, sv.SpecificAngularMomentum().Magnitude());
            Assert.Equal(0.0, sv.SpecificAngularMomentum().X);
            Assert.Equal(0.0, sv.SpecificAngularMomentum().Y);
            Assert.Equal(4458080000.0, sv.SpecificAngularMomentum().Z);
        }

        [Fact]
        public void SpecificOrbitalEnergyMomentum()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(-6.116559469556896E+06, -1.546174698676721E+06, 2.521950157430313E+06),
                new Vector3(-8.078523150700097E+02, -5.477647950892673E+03, -5.297615757935174E+03), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(-29305465.58806747, sv.SpecificOrbitalEnergy()); //ISS orbital energy in MJ
        }

        [Fact]
        public void Inclination()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 5.0, 5.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(System.Math.PI / 4.0, sv.Inclination());
        }

        [Fact]
        public void SemiMajorAxis()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(8000000.0, 0.0, 0.0), new Vector3(0.0, 6000.0, 6000.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(14415872.19306759, sv.SemiMajorAxis());
        }

        [Fact]
        public void AscendingNodeVector()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(8000000.0, 0.0, 0.0), new Vector3(0.0, 6000.0, 0.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            var v = sv.AscendingNodeVector().Normalize();
            Assert.Equal(1.0, v.X);
            Assert.Equal(0.0, v.Y);
            Assert.Equal(0.0, v.Z);
        }

        [Fact]
        public void AscendingNode()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(9208000.0, 3352000, 0.0), new Vector3(-1750, 4830, 5140), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(20.00308830929978, sv.AscendingNode() * IO.Astrodynamics.Constants.Rad2Deg);

            sv = new StateVector(new Vector3(7507000, -6299000, 0.0), new Vector3(3300, 3930, 5140), earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Assert.Equal(320.0005416342622, sv.AscendingNode() * IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void ArgumentOfPeriapis()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(8237000.0, 17000.0, 5308000.0), new Vector3(-2000.0, 6000.0, 3000.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(53.621820299859436, sv.ArgumentOfPeriapsis() * IO.Astrodynamics.Constants.Rad2Deg);

            sv = new StateVector(new Vector3(3973000.0, -4881000.0, -931000.0), new Vector3(5400.0, 3360.0, 5890.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(350.51061587951995, sv.ArgumentOfPeriapsis() * IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void TrueAnomaly()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(5070000.0, -2387000.0, 1430000.0), new Vector3(2450.0, 6350.0, 6440.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(30.57538215436613, sv.TrueAnomaly() * IO.Astrodynamics.Constants.Rad2Deg);

            sv = new StateVector(new Vector3(1664000.0, -4862000.0, -2655000.0), new Vector3(7520.0, 890, 5520.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(329.477000818209, sv.TrueAnomaly() * IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void EccentricAnomaly()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(6700000.0, 2494000.0, 0.0), new Vector3(-2150.0, 8850.0, 0.0), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(0.20776173316152752, sv.EccentricAnomaly());
        }

        [Fact]
        public void MeanAnomaly()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(-5775.068936894231E+03, -3372.353197848874E+03, 651.695854037289E+03),
                new Vector3(-0.661469579672604E+03, -7.147573777688288E+03, -2.915719736461653E+03), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(59.99823497591805, sv.MeanAnomaly() * IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void Period()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(-5775.068936894231E+03, -3372.353197848874E+03, 651.695854037289E+03),
                new Vector3(-0.661469579672604E+03, -7.147573777688288E+03, -2.915719736461653E+03), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(1.5501796754722221, sv.Period().TotalHours);
        }

        [Fact]
        public void MeanMotion()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            StateVector sv = new StateVector(new Vector3(-5775.068936894231E+03, -3372.353197848874E+03, 651.695854037289E+03),
                new Vector3(-0.661469579672604E+03, -7.147573777688288E+03, -2.915719736461653E+03), earth, DateTime.UtcNow, Frames.Frame.ICRF);

            Assert.Equal(0.0011258883596591215, sv.MeanMotion());
        }

        [Fact]
        public void ToFrame()
        {
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            //J2000->Ecliptic
            //Earth from sun at 0 TDB
            var sv = new StateVector(new Vector3(-26499033.67742509, 132757417.33833946, 57556718.47053819), new Vector3(-29.79426007, -5.01805231, -2.17539380), earth,
                new DateTime(2000, 1, 1, 12, 0, 0), Frames.Frame.ICRF);

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
            CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);

            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);
            var earthFrame = new Frames.Frame(PlanetsAndMoons.EARTH.Frame);

            //J2000->IAU_EARTH
            //Earth from sun at 0 TDB
            var sv = new StateVector(new Vector3(-26499033.67742509, 132757417.33833946, 57556718.47053819), new Vector3(-29.79426007, -5.01805231, -2.17539380), earth,
                epoch.ToTDB(), Frames.Frame.ICRF);

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
            var earth = TestHelpers.EarthAtJ2000;
            var sv = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 8000.0, 0.0), earth, earth.InitialOrbitalParameters.Epoch, Frames.Frame.ICRF);
            Assert.Equal(8000.0, sv.PerigeeVelocity(), 6);
        }

        [Fact]
        public void ApogeeVelocity()
        {
            var earth = TestHelpers.EarthAtJ2000;
            var sv = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 8000.0, 0.0), earth, earth.InitialOrbitalParameters.Epoch, Frames.Frame.ICRF);
            Assert.Equal(6654.42777338588, sv.ApogeeVelocity());
        }

        [Fact]
        public void Equality()
        {
            var earth = TestHelpers.EarthAtJ2000;
            var sv = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 8000.0, 0.0), earth, earth.InitialOrbitalParameters.Epoch, Frames.Frame.ICRF);
            var sv2 = new StateVector(new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 8000.0, 0.0), earth, earth.InitialOrbitalParameters.Epoch, Frames.Frame.ICRF);
            var sv3 = new StateVector(new Vector3(6900000.0, 0.0, 0.0), new Vector3(0.0, 8000.0, 0.0), earth, earth.InitialOrbitalParameters.Epoch, Frames.Frame.ICRF);
            Assert.Equal(sv, sv2);
            Assert.Equal(sv, (object)sv2);
            Assert.False(sv.Equals((object)null));
            Assert.True(sv.Equals((object)sv2));
            Assert.True(sv.Equals((object)sv));
            Assert.False(sv.Equals(null));
            Assert.True(sv.Equals(sv2));
            Assert.True(sv.Equals(sv));
            Assert.NotEqual(sv3, sv2);
            Assert.True(sv == sv2);
            Assert.True(sv3 != sv2);
        }
        
        [Fact]
        public void ToKeplerian()
        {
            var earth = TestHelpers.EarthAtJ2000;
            var ke = earth.InitialOrbitalParameters.ToKeplerianElements();
            // Assert.Equal(new KeplerianElements(),res);
        }
    }
}