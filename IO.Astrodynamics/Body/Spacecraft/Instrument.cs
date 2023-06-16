using System;
using IO.Astrodynamics.Math;

namespace IO.Astrodynamics.Body.Spacecraft
{
    public class Instrument : INaifObject
    {
        /// <summary>
        /// Naif identifier
        /// </summary>
        public int NaifId { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Model name
        /// </summary>
        public string Model { get; }

        /// <summary>
        /// Field of view
        /// </summary>
        public double FieldOfView { get; }

        /// <summary>
        /// Cross angle used for elliptical and rectangular shape
        /// </summary>
        public double CrossAngle { get; }

        /// <summary>
        /// Shape
        /// </summary>
        public InstrumentShape Shape { get; }

        /// <summary>
        /// Boresight vector
        /// </summary>
        public Vector3 Boresight { get; }

        /// <summary>
        /// Ref vector in the boresight plane
        /// </summary>
        public Vector3 RefVector { get; }

        public Spacecraft Spacecraft { get; }

        /// <summary>
        /// Instrument constructor
        /// </summary>
        /// <param name="naifId"></param>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <param name="fieldOfView"></param>
        /// <param name="shape"></param>
        /// <param name="boresight"></param>
        /// <param name="refVector"></param>
        /// <param name="crossAngle"></param>
        /// <exception cref="ArgumentException"></exception>
        public Instrument(int naifId, string name, string model, double fieldOfView, InstrumentShape shape, Vector3 boresight, Vector3 refVector,
            double crossAngle = double.NaN)
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

            if (naifId >= 0) throw new ArgumentOutOfRangeException(nameof(naifId));

            Name = name;
            Model = model;
            FieldOfView = fieldOfView;
            Shape = shape;
            Boresight = boresight;
            RefVector = refVector;
            CrossAngle = crossAngle;
            NaifId = naifId;
        }
    }
}