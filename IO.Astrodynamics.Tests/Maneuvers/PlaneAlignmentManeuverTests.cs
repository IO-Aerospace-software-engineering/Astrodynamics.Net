using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers
{
    public class PlaneAlignmentManeuverTests
    {
        [Fact]
        public void Create()
        {
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sun, new DateTime(2021, 01, 01), Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk1, ke);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021","sn1", 4000.0,4000.0);
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);
            spc1.AddFuelTank(fuelTank10);
            spc1.AddFuelTank(fuelTank11);
            spc1.AddPayload(pl1);
            spc1.AddEngine(eng);


            var targetOrbit = new KeplerianElements(150000000.0, 1.0, 0.0, 0.0, 0.0, 0.0, sun, new DateTime(2021, 01, 01), Frames.Frame.ECLIPTIC);

            PlaneAlignmentManeuver planeAlignmentManeuver = new PlaneAlignmentManeuver(spc1, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), targetOrbit, spc1.Engines.First());

            Assert.Single(planeAlignmentManeuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), planeAlignmentManeuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), planeAlignmentManeuver.MinimumEpoch);
            Assert.Equal(targetOrbit, planeAlignmentManeuver.TargetOrbit.AtEpoch(new DateTime(2021, 01, 01)));
        }
    }
}