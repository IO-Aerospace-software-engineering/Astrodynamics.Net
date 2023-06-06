using IO.Astrodynamics.Models.OrbitalParameters;
using System;

namespace IO.Astrodynamics.Models.Mission
{
    public interface ILocalizable
    {
        StateVector GetEphemeris(Frames.Frame frame, in DateTime epoch, int accuracy = 9);
        StateVector RelativeStateVector(Frames.Frame frame, BodyScenario targetBody, in DateTime epoch);
    }
}