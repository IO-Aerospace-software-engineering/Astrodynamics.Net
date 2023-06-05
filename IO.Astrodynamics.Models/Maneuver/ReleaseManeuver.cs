using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class ReleaseManeuver : Maneuver
    {
        ReleaseManeuver() : base() { }
        public ReleaseManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration) : base(spacecraft, minimumEpoch, maneuverHoldDuration)
        {
        }

        public override bool ComputeCanExecute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return this.Spacecraft.Child != null;
        }

        public override Vector3 ComputeDeltaV(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return Vector3.Zero;
        }

        public override Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return this.Spacecraft.GetOrientationFromICRF(maneuverPoint.Epoch).Orientation;
        }

        public override void Execute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {

        }
    }
}
