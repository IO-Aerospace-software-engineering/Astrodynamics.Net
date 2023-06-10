using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Integrator;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Integrator
{
    public class VVIntegratorTests
    {
        [Fact]
        public void Create()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);

            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth,
                new StateVector(new Vector3(-2.679537555216521E+07, 1.327011135216045E+08, 5.752533467064925E+07),
                    new Vector3(2.976558008982104E+01, -5.075339952746913E+00, -2.200929976753953E+00), sunScn, new DateTime(2021, 1, 1),
                    Frames.Frame.ICRF), scenario);

            CelestialBody moon = new CelestialBody(301, "moon", 4.902E+3, 6356.7519, 6378.1366);
            CelestialBodyScenario moonScn = new CelestialBodyScenario(moon,
                new StateVector(new Vector3(2.068864826237993E+05, 2.891146390982051E+05, 1.515746884380044E+05),
                    new Vector3(-8.366764389833921E-01, -5.602543663174073E-01, -1.710459390585548E-01), earthScn, new DateTime(2021, 1, 1),
                    Frames.Frame.ICRF), scenario);

            var sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 8.0, 0.0), earthScn, new DateTime(2021, 1, 1), Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, sv, scenario);

            VVIntegrator integrator = new VVIntegrator(sc, TimeSpan.FromSeconds(1.0));

            Assert.Equal(sc, integrator.Body);
            Assert.Equal(TimeSpan.FromSeconds(1.0), integrator.IntegrationStep);
        }

        [Fact]
        public void IntegrateGravity()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);

            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth,
                new StateVector(new Vector3(-2.679537555216521E+07, 1.327011135216045E+08, 5.752533467064925E+07),
                    new Vector3(2.976558008982104E+01, -5.075339952746913E+00, -2.200929976753953E+00), sunScn, new DateTime(2021, 1, 1),
                    Frames.Frame.ICRF), scenario);

            CelestialBody moon = new CelestialBody(301, "moon", 4.902E+3, 6356.7519, 6378.1366);
            CelestialBodyScenario moonScn = new CelestialBodyScenario(moon,
                new StateVector(new Vector3(2.068864826237993E+05, 2.891146390982051E+05, 1.515746884380044E+05),
                    new Vector3(-8.366764389833921E-01, -5.602543663174073E-01, -1.710459390585548E-01), earthScn, new DateTime(2021, 1, 1),
                    Frames.Frame.ICRF), scenario);

            earthScn.AddStateVector(new StateVector(new Vector3(-2.679537555216521E+07, 1.327011135216045E+08, 5.752533467064925E+07),
                new Vector3(2.976558008982104E+01, -5.075339952746913E+00, -2.200929976753953E+00), sunScn, new DateTime(2021, 1, 1).AddSeconds(1.0),
                Frames.Frame.ICRF));
            moonScn.AddStateVector(new StateVector(new Vector3(2.068864826237993E+05, 2.891146390982051E+05, 1.515746884380044E+05),
                new Vector3(-8.366764389833921E-01, -5.602543663174073E-01, -1.710459390585548E-01), earthScn, new DateTime(2021, 1, 1).AddSeconds(1.0),
                Frames.Frame.ICRF));
            var sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 8.0, 0.0), earthScn, new DateTime(2021, 1, 1), Frames.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, sv, scenario);

            VVIntegrator integrator = new VVIntegrator(sc, TimeSpan.FromSeconds(1.0), new GravityForce());

            var integratedSv = integrator.Integrate();

            Assert.Equal(6799.9956897156574, integratedSv.Position.X, 3);
            Assert.Equal(7.9999982033708893, integratedSv.Position.Y, 3);
            Assert.Equal(-0.00000069076103852024734, integratedSv.Position.Z, 3);
            Assert.Equal(-8.620565236076974 * 1E-3, integratedSv.Velocity.X, 3);
            Assert.Equal(7.9999913360235832, integratedSv.Velocity.Y, 3);
            Assert.Equal(-0.000001381498705046451, integratedSv.Velocity.Z, 3);
        }
    }
}