using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class PlaneAlignmentManeuver : ImpulseManeuver
    {
        public double? RelativeInclination { get; private set; }
        public bool? ExecuteAtAscendingNode { get; private set; }
        public bool? ExecuteAtDescendingNode
        {
            get
            {
                return !(ExecuteAtAscendingNode ?? null);
            }
        }

        PlaneAlignmentManeuver() : base() { }

        protected PlaneAlignmentManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        public PlaneAlignmentManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetBody, engines)
        {
        }

        public PlaneAlignmentManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public override bool ComputeCanExecute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            var targetOrbit = GetTargetOrbit(maneuverPoint.Epoch);

            var spacecraftSv = maneuverPoint.ToStateVector();

            if (System.Math.Abs(spacecraftSv.Position.Angle(targetOrbit.SpecificAngularMomentum().Cross(maneuverPoint.SpecificAngularMomentum()))) < Constants.AngularTolerance)
            {
                ExecuteAtAscendingNode = true;
                return true;
            }

            if (System.Math.Abs(spacecraftSv.Position.Angle(targetOrbit.SpecificAngularMomentum().Cross(maneuverPoint.SpecificAngularMomentum()).Inverse())) < Constants.AngularTolerance)
            {
                ExecuteAtAscendingNode = false;
                return true;
            }

            return false;
        }

        public override Vector3 ComputeDeltaV(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            var currentvectorState = maneuverPoint.ToStateVector();
            var targetOrbit = GetTargetOrbit(maneuverPoint.Epoch);

            Vector3 vel = currentvectorState.Velocity;
            Vector3 pos = currentvectorState.Position;

            //Project vector
            Vector3 projectedVector = vel - (pos * ((vel * pos) / (pos * pos)));

            //Compute relative inclination
            RelativeInclination = System.Math.Acos(System.Math.Cos(maneuverPoint.Inclination()) * System.Math.Cos(targetOrbit.Inclination()) + System.Math.Sin(maneuverPoint.Inclination()) * System.Math.Sin(targetOrbit.Inclination()) * System.Math.Cos(targetOrbit.AscendingNode() - maneuverPoint.AscendingNode()));

            double rotationAngle = Constants.PI2 + RelativeInclination.Value * 0.5;

            if (ExecuteAtAscendingNode == true)
            {
                rotationAngle = -rotationAngle;
            }

            //Compute deltaV
            double deltaV = 2.0 * projectedVector.Magnitude() * System.Math.Sin(RelativeInclination.Value * 0.5);

            //Compute the quaternion
            Quaternion q = new Quaternion(pos.Normalize(), rotationAngle);

            //Rotate velocity vector
            var rotateVecor = projectedVector.Normalize().Rotate(q);

            //Compute delta V vector
            return rotateVecor * deltaV;
        }

        public override Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            Vector3 targetOrientation = maneuverPoint.SpecificAngularMomentum();
            if (ExecuteAtAscendingNode == true)
            {
                targetOrientation = targetOrientation.Inverse();
            }

            return SpacecraftScenario.Front.To(targetOrientation);
        }
    }
}