using System;
using System.Collections.Generic;

namespace IO.Astrodynamics.Body.Spacecraft
{
    public class SpacecraftEngine 
    {
        public Spacecraft Spacecraft { get; private set; }
        public Engine Engine { get; }
        public SpacecraftFuelTank FuelTank { get; }
        public IReadOnlyCollection<Maneuver.Maneuver> Maneuvers { get; }
        public string SerialNumber { get;}

        public SpacecraftEngine(Spacecraft spacecraft, Engine engine, SpacecraftFuelTank fuelTank, string serialNumber)
        {
            if (spacecraft == null)
            {
                throw new ArgumentException("SpacecraftEngine requires a spacecraft");
            }

            if (engine == null)
            {
                throw new ArgumentException("SpacecraftEngine requires an engine");
            }

            if (fuelTank == null)
            {
                throw new ArgumentException("SpacecraftEngine requires a fuel tank");
            }

            Spacecraft = spacecraft;
            Engine = engine;
            FuelTank = fuelTank;
            if (serialNumber != null) SerialNumber = serialNumber;
        }

        public double BurnFuel(TimeSpan duration)
        {
            double fuelBurned = Engine.FuelFlow * duration.TotalSeconds;
            FuelTank.BurnFuel(fuelBurned);
            return fuelBurned;
        }

    }
}