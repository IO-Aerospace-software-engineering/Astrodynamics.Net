using System;

namespace IO.Astrodynamics.Body.Spacecraft
{
    public class Engine 
    {

        public string Name { get; private set; }
        public string Model { get; private set; }
        public double ISP { get; private set; }
        public double FuelFlow { get; private set; }
        public double Thrust { get; private set; }
        public FuelTank FuelTank { get; }
        public string SerialNumber { get;}
        public Engine(string name, string model,  string serialNumber,double isp, double fuelFlow, FuelTank fuelTank) 
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Engine requires a name");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("Engine requires a model");
            }

            if (isp <= 0)
            {
                throw new ArgumentException("ISP must be a positive number");
            }

            if (fuelFlow <= 0)
            {
                throw new ArgumentException("Fuel flow must be a positive number");
            }

            Name = name;
            Model = model;
            ISP = isp;
            FuelFlow = fuelFlow;
            FuelTank = fuelTank;
            SerialNumber = serialNumber;
            Thrust = isp * fuelFlow *Constants.g0;
        }
    }
}