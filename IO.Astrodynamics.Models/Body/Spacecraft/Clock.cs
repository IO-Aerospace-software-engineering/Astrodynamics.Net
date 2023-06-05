using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Body.Spacecraft
{

    public class Clock : Entity
    {
        public string Name { get; private set; }
        public double Resolution { get; private set; }

        private HashSet<SpacecraftScenario> _spacecrafts = new();
        public IReadOnlyCollection<SpacecraftScenario> Spacecrafts { get; private set; }


        public Clock(string name, double resolution, int id = default) : base(id)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Clock requires a name");
            }

            if (resolution <= 0.0)
            {
                throw new ArgumentException("Resolution must be a positive number");
            }

            Name = name;
            Resolution = resolution;
        }
    }
}