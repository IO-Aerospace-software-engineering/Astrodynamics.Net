using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class Payload : Entity
    {
        private HashSet<SpacecraftScenario> _spacecrafts = new();
        public IReadOnlyCollection<SpacecraftScenario> Spacecrafts { get; private set; }
        public string Name { get; private set; }
        public double Mass { get; private set; }

        public Payload(string name, double mass, int id = default) : base(id)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Payload must have a name");
            }

            if (mass <= 0)
            {
                throw new ArgumentException("Payload must have a mass");
            }

            Name = name;
            Mass = mass;
        }
    }
}