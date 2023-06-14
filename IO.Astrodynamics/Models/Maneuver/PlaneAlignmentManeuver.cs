using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Mission;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class PlaneAlignmentManeuver : ImpulseManeuver
    {
        public double? RelativeInclination { get; private set; }
        public bool? ExecuteAtAscendingNode { get; private set; }

        public bool? ExecuteAtDescendingNode
        {
            get { return !(ExecuteAtAscendingNode ?? null); }
        }

        protected PlaneAlignmentManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft,
            minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        public PlaneAlignmentManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit,
            engines)
        {
        }
    }
}