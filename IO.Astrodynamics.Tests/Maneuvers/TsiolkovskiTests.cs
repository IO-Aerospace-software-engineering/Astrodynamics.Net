using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Maneuvers
{
    public class TsiolkovskiTests
    {
        [Fact]
        public void DeltaM()
        {
            Assert.Equal(1000.0, IO.Astrodynamics.Models.Maneuver.Maneuver.ComputeDeltaM(300.0, 3000.0, 1.192876320728679), 9);
        }

        [Fact]
        public void DeltaT()
        {
            Assert.Equal(TimeSpan.FromSeconds(10.0), IO.Astrodynamics.Models.Maneuver.Maneuver.ComputeDeltaT(300.0, 3000.0, 100.0, 1.192876320728679));
        }

        [Fact]
        public void DeltaV()
        {
            double deltaV = IO.Astrodynamics.Models.Maneuver.Maneuver.ComputeDeltaV(300.0, 3000.0, 2000.0);

            Assert.Equal(1.192876320728679, deltaV);
        }
    }
}
