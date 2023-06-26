using System;
using IO.Astrodynamics.Body.Spacecraft;

namespace IO.Astrodynamics.Maneuver
{
    public class PhasingManeuver : ImpulseManeuver
    {
        public double TargetTrueLongitude { get; } = double.NaN;
        public uint RevolutionNumber { get; }

        public PhasingManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, uint revolutionNumber, params Engine[] engines) : base( minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            RevolutionNumber = revolutionNumber;
        }

        public PhasingManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double trueLongitude, uint revolutionNumber, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetTrueLongitude = trueLongitude;
            RevolutionNumber = revolutionNumber;
        }
    }
}