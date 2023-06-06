using System;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Maneuver
{
    public abstract class ImpulseManeuver : Maneuver
    {
        public Vector3 DeltaV { get; private set; }

        protected ImpulseManeuver() : base() { }
        protected ImpulseManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        protected ImpulseManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetBody, engines)
        {
        }

        protected ImpulseManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        /// <summary>
        /// Execute maneuver at epoch
        /// </summary>
        /// <param name="epoch"></param>
        public override void Execute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            //DeltaV
            DeltaV = ComputeDeltaV(maneuverPoint);

            //Orientation
            var orientation = ComputeOrientation(maneuverPoint);

            //Spread thrust
            SpreadThrust(maneuverPoint);

            var sv = maneuverPoint.ToStateVector();
            //Add new ephemeris and orientation
            Spacecraft.AddStateVector(new StateVector(sv.Position, sv.Velocity + DeltaV, maneuverPoint.CenterOfMotion, maneuverPoint.Epoch, maneuverPoint.Frame));
            Spacecraft.AddStateOrientationFromICRF(new StateOrientation(orientation, Vector3.Zero, maneuverPoint.Epoch, maneuverPoint.Frame));


            //Switch to next maneuver
            Spacecraft.SetStandbyManeuver(NextManeuver);
            if (Spacecraft.StandbyManeuver != null)
            {
                Spacecraft.StandbyManeuver.Handle(GetGlobalWindow().EndDate);
            }
        }

        protected virtual void SpreadThrust(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            TimeSpan deltaT = ComputeDeltaT(Spacecraft.GetTotalISP(), Spacecraft.PhysicalBody.GetTotalMass(), Spacecraft.Engines.Sum(x => x.Engine.FuelFlow), DeltaV.Magnitude());

            //Burned fuel
            foreach (var engine in Spacecraft.Engines)
            {
                engine.BurnFuel(deltaT);//If not enought fuel an exception is thrown
            }

            //Thrust window
            ThrustWindow = new Window(maneuverPoint.Epoch - deltaT * 0.66, deltaT);//2/3 before maneuver point and 1/3 after maneuver point

            //Attitude window equal at least thrust
            AttitudeWindow = ThrustWindow;
        }
    }
}