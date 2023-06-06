using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Models.Math;

namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class Spacecraft : Body
    {
        public double DryOperatingMass { get => base.Mass; }
        public double MaximumOperatingMass { get; }

        public Spacecraft(int naifId, string name, double mass, double maximumOperatingMass) : base(naifId, name, mass)
        {
            MaximumOperatingMass = maximumOperatingMass;
        }

        public override double GetTotalMass()
        {
            return DryOperatingMass;
        }
    }
}