using System;

using System.Collections.Generic;
using IO.Astrodynamics.Models.Surface;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Mission
{
    public class Scenario 
    {
        public string Name { get; private set; }
        public Window Window { get; private set; }
        public Mission Mission { get; private set; }
        private readonly HashSet<BodyScenario> _bodies = new();
        public IReadOnlyCollection<BodyScenario> Bodies => _bodies;
        private readonly HashSet<Site> _sites = new();
        public IReadOnlyCollection<Site> Sites => _sites;
        private readonly API _api;

        public Scenario(string name, Mission mission, in Window window, API api, int id = default) 
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            Name = name;
            Mission = mission ?? throw new System.ArgumentNullException(nameof(mission));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            Window = window;
            
        }

        public void AddBody(BodyScenario body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            _bodies.Add(body);
        }

        public void Propagate()
        {
            _api.ExecuteScenario(this);
        }
    }
}