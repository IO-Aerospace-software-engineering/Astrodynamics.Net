using System;
using IO.Astrodynamics.Models.Body.Spacecraft;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class PhasingManeuver : ImpulseManeuver
    {
        public double TargetTrueLongitude { get; private set; } = double.NaN;
        public uint RevolutionNumber { get; private set; }

        public PhasingManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, uint revolutionNumber, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            RevolutionNumber = revolutionNumber;
        }

        public PhasingManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double trueLongitude, uint revolutionNumber, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetTrueLongitude = trueLongitude;
            RevolutionNumber = revolutionNumber;
        }
    }
}