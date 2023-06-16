using System;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Mission
{
    public class ScenarioTests
    {
        [Fact]
        public void Create()
        {
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Mission1");
            Scenario scenario = new Scenario("Scenario", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            Assert.Equal("Scenario", scenario.Name);
            Assert.Equal(mission, scenario.Mission);
        }
    }
}