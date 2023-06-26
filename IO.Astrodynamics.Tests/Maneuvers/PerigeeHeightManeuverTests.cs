using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers
{
    public class PerigeeHeightManeuverTests
    {
        [Fact]
        public void Create()
        {
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);

            PerigeeHeightManeuver perigeeHeightManeuver = new PerigeeHeightManeuver(new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 151000000.0, eng);

            Assert.Single(perigeeHeightManeuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), perigeeHeightManeuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), perigeeHeightManeuver.MinimumEpoch);
            Assert.Equal(151000000.0, perigeeHeightManeuver.TargetPerigeeHeight);
        }
    }
}