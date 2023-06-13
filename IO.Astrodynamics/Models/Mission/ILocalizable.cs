using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Mission
{
    public interface ILocalizable : INaifObject
    {
        IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, CelestialBodyScenario observer, Frames.Frame frame, Aberration aberration,
            TimeSpan stepSize);

    }
}