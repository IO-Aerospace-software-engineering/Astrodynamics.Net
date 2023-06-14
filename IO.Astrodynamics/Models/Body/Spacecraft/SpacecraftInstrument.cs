using System;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;


namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class SpacecraftInstrument 
    {
        public Spacecraft Spacecraft { get; }
        public Instrument Instrument { get; }
        public Vector3 Orientation { get; }

        private API _api = new();
        

        private SpacecraftInstrument() { }
        public SpacecraftInstrument(Spacecraft spacecraft, Instrument instrument, Vector3 orientation)
        {
            if (spacecraft == null)
            {
                throw new ArgumentNullException("SpacecraftInstrument must have a spacecraft");
            }

            if (instrument == null)
            {
                throw new ArgumentNullException("SpacecraftInstrument must have an instrument");
            }

            Spacecraft = spacecraft;
            Instrument = instrument;
            Orientation = orientation;
        }
    }
}