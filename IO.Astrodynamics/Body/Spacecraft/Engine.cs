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
        public Engine(string name, string model, double isp, double fuelFlow) 
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
            Thrust = isp * fuelFlow *Constants.g0;
        }
    }
}