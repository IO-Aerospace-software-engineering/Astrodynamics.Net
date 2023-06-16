using System;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;


namespace IO.Astrodynamics.Body.Spacecraft
{
    public class Spacecraft : Body
    {
        public static readonly Vector3 Front = Vector3.VectorY;
        public static readonly Vector3 Back = Front.Inverse();
        public static readonly Vector3 Right = Vector3.VectorX;
        public static readonly Vector3 Left = Right.Inverse();
        public static readonly Vector3 Up = Vector3.VectorZ;
        public static readonly Vector3 Down = Up.Inverse();

        public Maneuver.Maneuver StandbyManeuver { get; private set; }
        public Spacecraft Parent { get; private set; }
        public Spacecraft Child { get; private set; }
        public Clock Clock { get; private set; }

        private HashSet<SpacecraftInstrument> _instruments = new();
        public IReadOnlyCollection<SpacecraftInstrument> Intruments => _instruments;

        private HashSet<SpacecraftFuelTank> _fuelTanks = new();
        public IReadOnlyCollection<SpacecraftFuelTank> FuelTanks => _fuelTanks;

        private HashSet<SpacecraftEngine> _engines = new();
        public IReadOnlyCollection<SpacecraftEngine> Engines => _engines;

        private HashSet<Payload> _payloads = new();
        public IReadOnlyCollection<Payload> Payloads => _payloads;
        public double DryOperatingMass
        {
            get => Mass;
        }

        public double MaximumOperatingMass { get; }

        public Spacecraft(int naifId, string name, double mass, double maximumOperatingMass,Clock clock, OrbitalParameters.OrbitalParameters initialOrbitalParameters) : base(naifId, name, mass,initialOrbitalParameters,new Frame($"{name}_SPACECRAFT"))
        {
            if (maximumOperatingMass < mass) throw new ArgumentOutOfRangeException(nameof(maximumOperatingMass));
            if (naifId >= 0) throw new ArgumentOutOfRangeException(nameof(naifId));
            MaximumOperatingMass = maximumOperatingMass;
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }
        
        /// <summary>
        /// Update clock
        /// </summary>
        /// <param name="clock"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateClock(Clock clock)
        {
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Add instrument to spacecraft
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="orientation"></param>
        public void AddInstrument(Instrument instrument, Vector3 orientation)
        {
            _instruments.Add(new SpacecraftInstrument(this, instrument, orientation));
        }

        /// <summary>
        /// Add engine to spacecraft
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="fuelTank"></param>
        /// <param name="serialNumber"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddEngine(Engine engine, FuelTank fuelTank, string serialNumber)
        {
            if (!FuelTanks.Select(x => x.FuelTank).Contains(fuelTank))
            {
                throw new InvalidOperationException(
                    "Unknown fuel tank, you must add fuel tank to spacecraft before add engine");
            }

            _engines.Add(new SpacecraftEngine(this, engine, FuelTanks.First(x => x.FuelTank == fuelTank), serialNumber));
        }

        /// <summary>
        /// Add fuel tank to spacecraft
        /// </summary>
        /// <param name="fuelTank"></param>
        /// <param name="quantity"></param>
        /// <param name="serialNumber"></param>
        public void AddFuelTank(FuelTank fuelTank, double quantity, string serialNumber)
        {
            _fuelTanks.Add(new SpacecraftFuelTank(this, fuelTank, quantity, serialNumber));
        }

        public void RemoveFuelTank(SpacecraftFuelTank fuelTank)
        {
            _fuelTanks.Remove(fuelTank);
        }

        /// <summary>
        /// Add payload to spacecraft
        /// </summary>
        /// <param name="payload"></param>
        public void AddPayload(Payload payload)
        {
            _payloads.Add(payload);
        }

        /// <summary>
        /// Set child spacecraft
        /// </summary>
        /// <param name="spacecraft"></param>
        public void SetChild(Spacecraft spacecraft)
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
        public void SetParent(Spacecraft spacecraft)
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
        public override double GetTotalMass()
        {
            return DryOperatingMass + FuelTanks.Sum(x => x.Quantity) + Payloads.Sum(x => x.Mass) +
                   (Child != null ? Child.GetTotalMass() : 0.0);
        }

        /// <summary>
        /// Get total ISP of this spacecraft
        /// </summary>
        /// <returns></returns>
        public double GetTotalISP()
        {
            return (Engines.Where(x => x.FuelTank.Quantity > 0.0).Sum(x => x.Engine.Thrust) /Constants.g0) /
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
        /// Put a maneuver in standby
        /// </summary>
        /// <param name="maneuver"></param>
        public void SetStandbyManeuver(Maneuver.Maneuver maneuver)
        {
            StandbyManeuver = maneuver;
        }

        public Dictionary<int, Maneuver.Maneuver> GetManeuvers()
        {
            Dictionary<int, Maneuver.Maneuver> maneuvers = new Dictionary<int, Maneuver.Maneuver>();

            var maneuver = StandbyManeuver;
            int order = 0;
            while (maneuver != null)
            {
                maneuvers[order] = maneuver;
                maneuver = maneuver.NextManeuver;
                order++;
            }

            return maneuvers;
        }
    }
}