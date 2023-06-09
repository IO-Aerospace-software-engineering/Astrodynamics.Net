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
        public Vector3 Boresight { get; }
        public Vector3 RefVector { get; }
        
        public Instrument(uint naifId, string name, string model, double fieldOfView, InstrumentShape shape, Vector3 boresight, Vector3 refVector, double crossAngle = double.NaN)
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
            Boresight = boresight;
            RefVector = refVector;
            CrossAngle = crossAngle;
            NaifId = (int)naifId;
        }
    }
}