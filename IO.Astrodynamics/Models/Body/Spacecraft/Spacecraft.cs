using System;

namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class Spacecraft : Body
    {
        public double DryOperatingMass
        {
            get => Mass;
        }

        //TODO Check if the MOM is not exceeded
        public double MaximumOperatingMass { get; }

        public Spacecraft(int naifId, string name, double mass, double maximumOperatingMass) : base(naifId, name, mass)
        {
            if (maximumOperatingMass < mass) throw new ArgumentOutOfRangeException(nameof(maximumOperatingMass));
            MaximumOperatingMass = maximumOperatingMass;
        }

        public override double GetTotalMass()
        {
            return DryOperatingMass;
        }
    }
}