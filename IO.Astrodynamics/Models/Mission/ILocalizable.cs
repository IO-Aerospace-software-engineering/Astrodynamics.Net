using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Mission
{
    public interface ILocalizable : INaifObject
    {
        IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, CelestialBody observer, Frame frame, Aberration aberration, TimeSpan stepSize);
        OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, CelestialBody observer, Frame frame, Aberration aberration);
        Frame Frame { get; }
    }
}