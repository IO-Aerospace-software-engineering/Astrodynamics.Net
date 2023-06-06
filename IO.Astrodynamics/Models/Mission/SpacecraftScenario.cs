using System;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Mission
{
    public class SpacecraftScenario : BodyScenario
    {
        public static readonly Vector3 Front = Vector3.VectorY;
        public static readonly Vector3 Back = Front.Inverse();
        public static readonly Vector3 Right = Vector3.VectorX;
        public static readonly Vector3 Left = Right.Inverse();
        public static readonly Vector3 Up = Vector3.VectorZ;
        public static readonly Vector3 Down = Up.Inverse();

        public new Spacecraft PhysicalBody
        {
            get => base.PhysicalBody as Spacecraft;
            private set => base.PhysicalBody = value;
        }

        public Maneuver.Maneuver StandbyManeuver { get; private set; }
        public SpacecraftScenario Parent { get; private set; }
        public SpacecraftScenario Child { get; private set; }
        public Clock Clock { get; private set; }


        private HashSet<SpacecraftInstrument> _instruments = new();
        public IReadOnlyCollection<SpacecraftInstrument> Intruments => _instruments;

        private HashSet<SpacecraftFuelTank> _fuelTanks = new();
        public IReadOnlyCollection<SpacecraftFuelTank> FuelTanks => _fuelTanks;

        private HashSet<SpacecraftEngine> _engines = new();
        public IReadOnlyCollection<SpacecraftEngine> Engines => _engines;

        private HashSet<Payload> _payloads = new();
        public IReadOnlyCollection<Payload> Payloads => _payloads;

        private SpacecraftScenario() : base()
        {
        }

        /// <summary>
        /// Create new spacecraft to simulate scenario
        /// </summary>
        /// <param name="spacecraft"></param>
        /// <param name="clock"></param>
        /// <param name="standbyManeuver"></param>
        /// <param name="initialOrbitalParameters"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public SpacecraftScenario(Spacecraft spacecraft, Clock clock,
            OrbitalParameters.OrbitalParameters initialOrbitalParameters, Scenario scenario, int id = 0) : base(
            spacecraft,
            initialOrbitalParameters, new Frames.Frame($"frm_{spacecraft.Name}"), scenario, id)
        {
            if (spacecraft == null)
            {
                throw new ArgumentException("SpacecraftScenario requires a spacecraft body");
            }

            if (clock is null)
            {
                throw new ArgumentNullException(nameof(clock));
            }

            PhysicalBody = spacecraft;
            Clock = clock;
        }

        /// <summary>
        /// Update clock
        /// </summary>
        /// <param name="clock"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateClock(Clock clock)
        {
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            Clock = clock;
        }

        /// <summary>
        /// Add instrument to spacraft
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="orientation"></param>
        public void AddInstrument(Instrument instrument, Quaternion orientation)
        {
            this._instruments.Add(new SpacecraftInstrument(this, instrument, orientation));
        }

        /// <summary>
        /// Add engine to spacecraft
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="fuelTank"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddEngine(Engine engine, FuelTank fuelTank)
        {
            if (!FuelTanks.Select(x => x.FuelTank).Contains(fuelTank))
            {
                throw new InvalidOperationException(
                    "Unknown fuel tank, you must add fuel tank to spacecraft before add engine");
            }

            this._engines.Add(new SpacecraftEngine(this, engine, FuelTanks.First(x => x.FuelTank == fuelTank)));
        }

        /// <summary>
        /// Add fuel tank to spacecraft
        /// </summary>
        /// <param name="fuelTank"></param>
        /// <param name="quantity"></param>
        public void AddFuelTank(FuelTank fuelTank, double quantity)
        {
            this._fuelTanks.Add(new SpacecraftFuelTank(this, fuelTank, quantity));
        }

        public void RemoveFuelTank(SpacecraftFuelTank fuelTank)
        {
            this._fuelTanks.Remove(fuelTank);
        }

        /// <summary>
        /// Add payload to spacecraft
        /// </summary>
        /// <param name="payload"></param>
        public void AddPayload(Payload payload)
        {
            this._payloads.Add(payload);
        }

        /// <summary>
        /// Set child spacecraft
        /// </summary>
        /// <param name="spacecraft"></param>
        public void SetChild(SpacecraftScenario spacecraft)
        {
            Child = spacecraft;

            if (spacecraft != null)
            {
                spacecraft.Parent = this;
            }
        }

        /// <summary>
        /// Set parent spacecraft
        /// </summary>
        /// <param name="spacecraft"></param>
        public void SetParent(SpacecraftScenario spacecraft)
        {
            Parent = spacecraft;
            if (spacecraft != null)
            {
                spacecraft.Child = this;
            }
        }

        /// <summary>
        /// Get Dry operating mass + fuel+ payloads + children
        /// </summary>
        /// <returns></returns>
        public double GetTotalMass()
        {
            return PhysicalBody.DryOperatingMass + FuelTanks.Sum(x => x.Quantity) + Payloads.Sum(x => x.Mass) +
                   (Child != null ? Child.GetTotalMass() : 0.0);
        }

        /// <summary>
        /// Get total ISP of this spacecraft
        /// </summary>
        /// <returns></returns>
        public double GetTotalISP()
        {
            return (Engines.Where(x => x.FuelTank.Quantity > 0.0).Sum(x => x.Engine.Thrust) / Constants.g0) /
                   GetTotalFuelFlow();
        }

        /// <summary>
        /// Get total fuel flow of this spacecraft
        /// </summary>
        /// <returns></returns>
        public double GetTotalFuelFlow()
        {
            return Engines.Where(x => x.FuelTank.Quantity > 0.0).Sum(x => x.Engine.FuelFlow);
        }

        /// <summary>
        /// Get total fuel of this spacecraft
        /// </summary>
        /// <returns></returns>
        public double GetTotalFuel()
        {
            return FuelTanks.Sum(x => x.Quantity);
        }


        /// <summary>
        /// Add state vector and propagate to children
        /// </summary>
        /// <param name="stateVector"></param>
        public override void AddStateVector(params StateVector[] stateVectors)
        {
            foreach (var stateVector in stateVectors)
            {
                base.AddStateVector(stateVector);
                if (Child != null)
                {
                    Child.AddStateVector(stateVector);
                }
            }
        }

        /// <summary>
        /// Put a maneuver in standby
        /// </summary>
        /// <param name="maneuver"></param>
        public void SetStandbyManeuver(Maneuver.Maneuver maneuver)
        {
            StandbyManeuver = maneuver;
        }
    }
}