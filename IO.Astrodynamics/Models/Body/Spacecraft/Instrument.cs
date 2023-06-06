using System;
using IO.Astrodynamics.Models.Math;


namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class Instrument : INaifObject
    {
        //TODO Comment clearly
        public int NaifId { get; }
        public string Name { get; }
        public string Model { get; }
        public double FieldOfView { get; }
        public double CrossAngle { get; }
        public InstrumentShape Shape { get; }
        public static readonly Vector3 Boresight = Vector3.VectorZ;
        public static readonly Vector3 RefVector = Vector3.VectorX;

        public Instrument(int naifId, string name, string model, double fieldOfView, InstrumentShape shape, double crossAngle = double.NaN)
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
            Shape = shape;
            CrossAngle = crossAngle;
            NaifId = naifId;
        }
    }
}