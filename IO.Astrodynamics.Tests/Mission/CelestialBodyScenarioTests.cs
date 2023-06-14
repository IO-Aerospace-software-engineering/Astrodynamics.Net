using System;
using System.Collections;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Mission
{
    public class CelestialBodyTests
    {
        private API _api = new API();

        public CelestialBodyTests()
        {
            _api.LoadKernels(Astrodynamics.Tests.Constants.SolarSystemKernelPath);
        }

        
    }
}