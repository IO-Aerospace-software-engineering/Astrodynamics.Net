using System;
using IO.Astrodynamics.Body.Spacecraft;

namespace IO.Astrodynamics.Maneuver
{
    public class PlaneAlignmentManeuver : ImpulseManeuver
    {
        public double? RelativeInclination { get; }
        public bool? ExecuteAtAscendingNode { get; }

        public bool? ExecuteAtDescendingNode
        {
            get { return !(ExecuteAtAscendingNode ?? null); }
        }

        protected PlaneAlignmentManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params Engine[] engines) : base(spacecraft,
            minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        public PlaneAlignmentManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params Engine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit,
            engines)
        {
        }
    }
}