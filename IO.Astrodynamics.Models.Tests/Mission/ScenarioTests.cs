using Xunit;
using IO.Astrodynamics.Models.Mission;
using System;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Tests.Mission
{
    public class ScenarioTests
    {
        [Fact]
        public void Create()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("Mission1");
            IO.Astrodynamics.Models.Mission.Scenario scenario = new IO.Astrodynamics.Models.Mission.Scenario("Scenario", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            Assert.Equal("Scenario", scenario.Name);
            Assert.Equal(mission, scenario.Mission);
        }
    }
}