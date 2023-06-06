using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Mission;


namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class SpacecraftEngine 
    {
        public SpacecraftScenario Spacecraft { get; private set; }
        public Engine Engine { get; private set; }
        public SpacecraftFuelTank FuelTank { get; private set; }
        public IReadOnlyCollection<Maneuver.Maneuver> Maneuvers { get; private set; }

        private SpacecraftEngine() { }
        public SpacecraftEngine(SpacecraftScenario spacecraft, Engine engine, SpacecraftFuelTank fuelTank)
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
        }

        public double BurnFuel(TimeSpan duration)
        {
            double fuelBurned = Engine.FuelFlow * duration.TotalSeconds;
            FuelTank.BurnFuel(fuelBurned);
            return fuelBurned;
        }

    }
}