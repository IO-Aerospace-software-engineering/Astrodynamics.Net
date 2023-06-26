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

        protected PlaneAlignmentManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params Engine[] engines) : base(
            minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        public PlaneAlignmentManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }
    }
}