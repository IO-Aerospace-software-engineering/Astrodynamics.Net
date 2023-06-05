using System;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class FuelTank : Entity
    {

        public string Name { get; private set; }
        public string Model { get; private set; }
        public double Capacity { get; private set; }

        public FuelTank(string name, string model, double capacity, int id = default) : base(id)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Fuel tank requires a name");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("Fuel tank requires a model");
            }

            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be a positive number");
            }

            Name = name;
            Model = model;
            Capacity = capacity;
        }

    }
}