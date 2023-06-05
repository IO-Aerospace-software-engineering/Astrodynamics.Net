using System;
using IO.Astrodynamics.Models.SeedWork;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Mission
{
    public class Scenario : Entity
    {
        public string Name { get; private set; }
        public Window Window { get; private set; }
        public Mission Mission { get; private set; }
        private readonly HashSet<BodyScenario> _bodies = new();
        public IReadOnlyCollection<BodyScenario> Bodies => _bodies;

        private Scenario() { }
        public Scenario(string name, Mission mission, in Window window, int id = default) : base(id)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            Name = name;
            Mission = mission ?? throw new System.ArgumentNullException(nameof(mission));
            Window = window;
        }

        public void AddBody(BodyScenario body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            _bodies.Add(body);
        }
    }
}