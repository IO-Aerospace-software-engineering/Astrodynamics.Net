using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.OrbitalParameters
{
    public class KeplerianElementsTests
    {
        [Fact]
        public void Create()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            DateTime epoch = DateTime.Now;
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 10.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, epoch, Frames.Frame.ICRF);
            Assert.Equal(20000.0, ke.SemiMajorAxis());
            Assert.Equal(0.5, ke.Eccentricity());
            Assert.Equal(30.0, ke.Inclination() *IO.Astrodynamics.Constants.Rad2Deg, 14);
            Assert.Equal(40.0, ke.AscendingNode() *IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(50.0, ke.ArgumentOfPeriapsis() *IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(10.0, ke.MeanAnomaly() *IO.Astrodynamics.Constants.Rad2Deg);
            Assert.Equal(earth, ke.CenterOfMotion);
            Assert.Equal(epoch, ke.Epoch);
            Assert.Equal(Frames.Frame.ICRF, ke.Frame);
        }

        [Fact]
        public void ToStateVector()
        {
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(2E4, 0.43336, 30.193 *IO.Astrodynamics.Constants.Deg2Rad,
                44.6017 *IO.Astrodynamics.Constants.Deg2Rad, 30.68 *IO.Astrodynamics.Constants.Deg2Rad, 356.73 *IO.Astrodynamics.Constants.Deg2Rad, earth,
                DateTime.UtcNow, Frames.Frame.ICRF);
            StateVector sv = ke.ToStateVector();
            Assert.Equal(5001.878051605426, sv.Position.X);
            Assert.Equal(10000.079990636958, sv.Position.Y);
            Assert.Equal(2099.2692094778067, sv.Position.Z);

            Assert.Equal(-5.991230167959389, sv.Velocity.X);
            Assert.Equal(1.926743625181508, sv.Velocity.Y);
            Assert.Equal(3.245991033815741, sv.Velocity.Z);
        }

        [Fact]
        public void TrueAnomaly10()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 10.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow, Frames.Frame.ICRF);
            double v = ke.TrueAnomaly();
            Assert.Equal(33.342843885635396, v *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void TrueAnomaly0()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 0.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow, Frames.Frame.ICRF);
            double v = ke.TrueAnomaly();
            Assert.Equal(0.0, v *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void TrueAnomaly180()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 180.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow,
                Frames.Frame.ICRF);
            double v = ke.TrueAnomaly();
            Assert.Equal(180.0, v *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void TrueAnomaly300()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 300.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow,
                Frames.Frame.ICRF);
            double v = ke.TrueAnomaly();
            Assert.Equal(241.18499907498312, v *IO.Astrodynamics.Constants.Rad2Deg);
        }

        [Fact]
        public void ExcentricityVector()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(7487.36, 0.0918, 0.0, 0.0, 0.0, 0.0 *IO.Astrodynamics.Constants.Deg2Rad,
                earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Vector3 ev = ke.EccentricityVector();
            Assert.Equal(0.0918000000000001, ev.Magnitude());
            Assert.Equal(0.0918000000000001, ev.X);
            Assert.Equal(0.0, ev.Y);
            Assert.Equal(0.0, ev.Z);
        }

        [Fact]
        public void AscendingNodeVector()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(8811.47, 0.228,IO.Astrodynamics.Constants.PI2 * 0.5, 0.0, 0.0,
                0.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Vector3 anv = ke.AscendingNodeVector().Normalize();
            Assert.Equal(1.0, anv.Magnitude());
            Assert.Equal(1.0, anv.X);
            Assert.Equal(0.0, anv.Y);
            Assert.Equal(0.0, anv.Z);
        }

        [Fact]
        public void DescendingNodeVector()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(8811.47, 0.228,IO.Astrodynamics.Constants.PI2 * 0.5, 0.0, 0.0,
                0.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow, Frames.Frame.ICRF);
            Vector3 anv = ke.DescendingNodeVector().Normalize();
            Assert.Equal(1.0, anv.Magnitude());
            Assert.Equal(-1.0, anv.X);
            Assert.Equal(0.0, anv.Y);
            Assert.Equal(0.0, anv.Z);
        }

        [Fact]
        public void EccentricAnomaly()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 180.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow,
                Frames.Frame.ICRF);
            double ea = ke.EccentricAnomaly();
            Assert.Equal(IO.Astrodynamics.Constants.PI, ea);
        }

        [Fact]
        public void SpecificAngularMomentum()
        {
            
            
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            
            KeplerianElements ke = new KeplerianElements(149753367.81178582, 0.0010241359778564595, 0.0, 0.0, 0.0, 0.0,
                sun, DateTime.UtcNow, Frames.Frame.ICRF);
            Vector3 sa = ke.SpecificAngularMomentum();
            Assert.Equal(0.0, sa.X);
            Assert.Equal(0.0, sa.Y);
            Assert.Equal(4458039254.889903, sa.Z);
        }

        [Fact]
        public void SpecificOrbitalEnergyMomentum()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(6800.81178582, 0.00134, 51.71 *IO.Astrodynamics.Constants.Deg2Rad,
                32.57 *IO.Astrodynamics.Constants.Deg2Rad, 105.64 *IO.Astrodynamics.Constants.Deg2Rad, 46.029 *IO.Astrodynamics.Constants.Deg2Rad, earth,
                DateTime.UtcNow, Frames.Frame.ICRF);
            double energy = ke.SpecificOrbitalEnergy();
            Assert.Equal(-29.30535753328009, energy);
        }

        [Fact]
        public void PerigeeVectorAnomaly()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 180.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow,
                Frames.Frame.ICRF);
            var pv = ke.PerigeeVector();
            Assert.Equal(10000.0, pv.Magnitude(), 9);
            Assert.Equal(659.6961052988252, pv.X);
            Assert.Equal(9213.80479648972, pv.Y);
            Assert.Equal(3830.222215594889, pv.Z);
        }

        [Fact]
        public void ApogeeVectorAnomaly()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements ke = new KeplerianElements(20000, 0.5, 30.0 *IO.Astrodynamics.Constants.Deg2Rad, 40.0 *IO.Astrodynamics.Constants.Deg2Rad,
                50.0 *IO.Astrodynamics.Constants.Deg2Rad, 180.0 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow,
                Frames.Frame.ICRF);
            var av = ke.ApogeeVector();
            Assert.Equal(30000.0, av.Magnitude(), 9);
            Assert.Equal(-1979.0883158964755, av.X);
            Assert.Equal(-27641.41438946916, av.Y);
            Assert.Equal(-11490.666646784666, av.Z);
        }

        [Fact]
        public void TrueAnomalyToMeanAnomaly()
        {
            
            
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            
            KeplerianElements km0 = new KeplerianElements(20000, 0.3, 0.0, 0.0, 0.0, 0.0, earth, DateTime.UtcNow,
                Frames.Frame.ICRF);
            KeplerianElements km90 = new KeplerianElements(20000, 0.3, 0.0, 0.0, 0.0, 90.0 *IO.Astrodynamics.Constants.Deg2Rad,
                earth, DateTime.UtcNow, Frames.Frame.ICRF);
            KeplerianElements km180 = new KeplerianElements(20000, 0.3, 0.0, 0.0, 0.0, 180.0 *IO.Astrodynamics.Constants.Deg2Rad,
                earth, DateTime.UtcNow, Frames.Frame.ICRF);
            KeplerianElements km270 = new KeplerianElements(20000, 0.3, 0.0, 0.0, 0.0, 270.0 *IO.Astrodynamics.Constants.Deg2Rad,
                earth, DateTime.UtcNow, Frames.Frame.ICRF);

            double v0 = km0.TrueAnomaly();
            double v90 = km90.TrueAnomaly();
            double v180 = km180.TrueAnomaly();
            double v270 = km270.TrueAnomaly();

            double m = km0.MeanAnomaly(v0);
            Assert.Equal(0.0, m);

            m = km90.MeanAnomaly(v90);
            Assert.Equal(89.99999997471907, m *IO.Astrodynamics.Constants.Rad2Deg, 12);

            m = km0.MeanAnomaly(v180);
            Assert.Equal(180.0, m *IO.Astrodynamics.Constants.Rad2Deg);

            m = km0.MeanAnomaly(v270);
            Assert.Equal(270.0000000252809, m *IO.Astrodynamics.Constants.Rad2Deg);
        }
    }
}