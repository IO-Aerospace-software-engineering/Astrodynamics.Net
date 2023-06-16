using Xunit;

namespace IO.Astrodynamics.Tests.Mission
{
    public class MissionTests
    {
        [Fact]
        public void Create()
        {
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Mission1");
            Assert.Equal("Mission1", mission.Name);
        }
    }
}