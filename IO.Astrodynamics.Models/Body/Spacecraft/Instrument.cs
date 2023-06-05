using System;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class Instrument : Entity
    {

        public string Name { get; private set; }
        public string Model { get; private set; }
        public double FieldOfView { get; private set; }

        public Instrument(string name, string model, double fieldOfView, int id = default) : base(id)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Instrument requires a name");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentException("Instrument requires a model");
            }

            if (fieldOfView <= 0)
            {
                throw new ArgumentException("fieldOfView must be a positive number");
            }

            Name = name;
            Model = model;
            FieldOfView = fieldOfView;
        }
    }
}