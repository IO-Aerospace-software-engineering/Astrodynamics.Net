using System;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Math
{
    public class LagrangeTests
    {
        [Fact]
        public void InterpolateSquare()
        {
            //Interpolate square function
            (double x, double y)[] data = new (double, double)[10];
            for (int i = 0; i < 10; i++)
            {
                data[i] = (i, i * i);
            }

            double res = Lagrange.Interpolate(data, 3);
            Assert.Equal(9.0, res);

            res = Lagrange.Interpolate(data, -3);
            Assert.Equal(9.0, res);

            res = Lagrange.Interpolate(data, 11);
            Assert.Equal(121.0, res, 11);
        }

        [Fact]
        public void InterpolateCubic()
        {
            //Interpolate square function
            (double x, double y)[] data = new (double, double)[10];
            for (int i = 0; i < 10; i++)
            {
                data[i] = (i, i * i * i);
            }

            double res = Lagrange.Interpolate(data, 3);
            Assert.Equal(27.0, res);

            res = Lagrange.Interpolate(data, -3);
            Assert.Equal(-27.0, res);

            res = Lagrange.Interpolate(data, 11);
            Assert.Equal(1331.0, res, 9);
        }

        [Fact]
        public void InterpolateStateVectorSquare()
        {

            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            //Interpolate square function
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScenario = new CelestialBodyScenario(earth, scenario);
            StateVector[] data = new StateVector[10];
            var start = new System.DateTime(2021, 01, 01, 0, 0, 0);
            for (int i = 0; i < 10; i++)
            {
                data[i] = new StateVector(new Vector3(i * i, 0.0, 0.0), new Vector3(i * i, 0.0, 0.0), earthScenario, start.AddSeconds(i), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            }

            var res = Lagrange.Interpolate(data, start.AddSeconds(3));
            Assert.Equal(9.0, res.Position.X);
            Assert.Equal(0.0, res.Position.Y);
            Assert.Equal(0.0, res.Position.Z);

            res = Lagrange.Interpolate(data, start.AddSeconds(-3));
            Assert.Equal(9.0, res.Position.X, 11);
            Assert.Equal(0.0, res.Position.Y);
            Assert.Equal(0.0, res.Position.Z);

            res = Lagrange.Interpolate(data, start.AddSeconds(11));
            Assert.Equal(121.0, res.Position.X, 10);
            Assert.Equal(0.0, res.Position.Y);
            Assert.Equal(0.0, res.Position.Z);
        }

        [Fact]
        public void InterpolateStateVectorCubic()
        {

            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            //Interpolate square function
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScenario = new CelestialBodyScenario(earth, scenario);
            StateVector[] data = new StateVector[10];
            var start = new System.DateTime(2021, 01, 01, 0, 0, 0);
            for (int i = 0; i < 10; i++)
            {
                data[i] = new StateVector(new Vector3(i * i * i, 0.0, 0.0), new Vector3(i * i * i, 0.0, 0.0), earthScenario, start.AddSeconds(i), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            }

            var res = Lagrange.Interpolate(data, start.AddSeconds(3));
            Assert.Equal(27.0, res.Position.X);
            Assert.Equal(0.0, res.Position.Y);
            Assert.Equal(0.0, res.Position.Z);

            res = Lagrange.Interpolate(data, start.AddSeconds(-3));
            Assert.Equal(-27.0, res.Position.X);
            Assert.Equal(0.0, res.Position.Y);
            Assert.Equal(0.0, res.Position.Z);

            res = Lagrange.Interpolate(data, start.AddSeconds(11));
            Assert.Equal(1331.0, res.Position.X, 9);
            Assert.Equal(0.0, res.Position.Y);
            Assert.Equal(0.0, res.Position.Z);
        }

        [Fact]
        public void InterpolateStateOrientation()
        {
            StateOrientation[] data = new StateOrientation[10];
            var start = new System.DateTime(2021, 01, 01, 0, 0, 0);
            for (int i = 0; i < 10; i++)
            {
                data[i] = new StateOrientation(new Quaternion(i * i, 1000.0 + i * i, 10000.0 + i * i, 100000 + i * i), new Vector3(i * i, 0.0, 0.0), start.AddSeconds(i), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            }
        }
    }
}