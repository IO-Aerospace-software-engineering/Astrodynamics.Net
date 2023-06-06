using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Coordinates
{
    public class EquatorialTests
    {
        [Fact]
        public void Create()
        {
            Equatorial eq = new Equatorial(1.0, 2.0, 3.0);
            Assert.Equal(new Equatorial(1.0, 2.0, 3.0), eq);
        }

        [Fact]
        public void CreateFromStateVector()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            var epoch = DateTime.MinValue;
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            Equatorial eq = new Equatorial(new StateVector(new Vector3(-291608.38463344, -266716.83339423, -76102.48709990), new Vector3(), earthScn, epoch, Frames.Frame.ICRF));
            Assert.Equal(new Equatorial(-0.19024413568211371, 3.8824377884371972, 402448.63988732797), eq);
        }

        [Fact]
        public void CreateFromStateVector2()
        {
            var epoch = new DateTime(2021, 1, 1);
            TestHelpers th = new TestHelpers();

            var moon = th.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var eq = new Equatorial(new StateVector(new Vector3(-202831.34150844064, 284319.70678317308, 150458.88140126597), new Vector3(-0.48702480142667454, -0.26438331399030518, -0.17175837261637006), earth, epoch, Frames.Frame.ICRF));
            Assert.Equal(new Equatorial(0.406773808779999, 2.1904536325374035, 380284.26703704614), eq);
        }
    }
}
