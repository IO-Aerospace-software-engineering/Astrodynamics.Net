using System;

namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class Engine 
    {

        public string Name { get; private set; }
        public string Model { get; private set; }
        public string SerialNumber { get; private set; }
        public double ISP { get; private set; }
        public double FuelFlow { get; private set; }
        public double Thrust { get; private set; }
        public Engine(string name, string model, string serialNumber, double iSP, double fuelFlow, int id = default) 
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Engine requires a name");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("Engine requires a model");
            }

            if (string.IsNullOrEmpty(serialNumber))
            {
                throw new ArgumentException($"'{nameof(serialNumber)}' cannot be null or empty.", nameof(serialNumber));
            }

            if (iSP <= 0)
            {
                throw new ArgumentException("ISP must be a positive number");
            }

            if (fuelFlow <= 0)
            {
                throw new ArgumentException("Fuel flow must be a positive number");
            }

            Name = name;
            Model = model;
            ISP = iSP;
            FuelFlow = fuelFlow;
            SerialNumber = serialNumber;
            Thrust = iSP * fuelFlow * Constants.g0;
        }
    }
}