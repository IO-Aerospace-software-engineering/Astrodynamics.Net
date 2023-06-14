using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Mission;


namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    //TODO create spacecraft payload
    public class Payload
    {
        private HashSet<Spacecraft> _spacecrafts = new();
        public IReadOnlyCollection<Spacecraft> Spacecrafts { get; private set; }
        public string Name { get; private set; }
        public string SerialNumber { get; private set; }
        public double Mass { get; private set; }

        public Payload(string name, double mass, string serialNumber)
        {
            if (mass <= 0) throw new ArgumentException("Payload must have a mass");


            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            if (string.IsNullOrEmpty(serialNumber)) throw new ArgumentException("Value cannot be null or empty.", nameof(serialNumber));

            Name = name;
            Mass = mass;
            SerialNumber = serialNumber;
        }
    }
}