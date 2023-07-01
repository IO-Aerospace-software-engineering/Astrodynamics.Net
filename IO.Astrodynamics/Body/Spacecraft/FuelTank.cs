using System;

namespace IO.Astrodynamics.Body.Spacecraft
{
    public class FuelTank : IEquatable<FuelTank>
    {
        public bool Equals(FuelTank other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Model == other.Model && SerialNumber == other.SerialNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FuelTank)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Model, SerialNumber);
        }

        public static bool operator ==(FuelTank left, FuelTank right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FuelTank left, FuelTank right)
        {
            return !Equals(left, right);
        }

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