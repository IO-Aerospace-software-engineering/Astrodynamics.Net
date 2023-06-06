using Xunit;
using IO.Astrodynamics.Models.Mission;
using System;

namespace IO.Astrodynamics.Models.Tests.Mission
{
    public class MissionTests
    {
        [Fact]
        public void Create()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("Mission1");
            Assert.Equal("Mission1", mission.Name);
        }
    }
}