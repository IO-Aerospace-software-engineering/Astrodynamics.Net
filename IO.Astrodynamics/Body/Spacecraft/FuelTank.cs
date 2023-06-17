using System;

namespace IO.Astrodynamics.Body.Spacecraft
{
    public class FuelTank
    {
        public string Name { get; }
        public string Model { get; }
        public double Capacity { get; }
        public double InitialQuantity { get; }
        public string SerialNumber { get; }

        public FuelTank(string name, string model, string serialNumber, double capacity, double initialQuantity)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            if (string.IsNullOrEmpty(model)) throw new ArgumentException("Value cannot be null or empty.", nameof(model));
            if (string.IsNullOrEmpty(serialNumber)) throw new ArgumentException("Value cannot be null or empty.", nameof(serialNumber));
            if (initialQuantity <= 0 || initialQuantity > capacity) throw new ArgumentOutOfRangeException(nameof(initialQuantity));
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            Name = name;
            Model = model;
            Capacity = capacity;
            InitialQuantity = initialQuantity;
            SerialNumber = serialNumber;
        }
    }
}