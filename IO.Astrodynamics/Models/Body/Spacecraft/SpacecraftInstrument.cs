using System;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;


namespace IO.Astrodynamics.Models.Body.Spacecraft
{
    public class SpacecraftInstrument 
    {
        public SpacecraftScenario Spacecraft { get; }
        public Instrument Instrument { get; }
        public Quaternion Orientation { get; }

        private SpacecraftInstrument() { }
        public SpacecraftInstrument(SpacecraftScenario spacecraft, Instrument instrument, Quaternion orientation)
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

        /// <summary>
        /// Know if body is in field of view
        /// </summary>
        /// <param name="bodyScenario">Target</param>
        /// <param name="epoch">AT epoch</param>
        /// <returns></returns>
        public bool IsInFieldOfView(BodyScenario bodyScenario, in DateTime epoch)
        {
            return IsInFieldOfView(Spacecraft.RelativeStateVector(bodyScenario, epoch));
        }

        /// <summary>
        /// Know if these orbital parameters are in field of view
        /// </summary>
        /// <param name="orbitalParameters">Target orbital parameters</param>
        /// <returns></returns>
        public bool IsInFieldOfView(OrbitalParameters.OrbitalParameters orbitalParameters)
        {
            var sv = orbitalParameters.ToFrame(Frames.Frame.ICRF).ToStateVector();
            var foresight = SpacecraftScenario.Front.Rotate(Spacecraft.GetOrientationFromICRF(orbitalParameters.Epoch).Rotation * Orientation);
            return sv.Position.Angle(foresight) < Instrument.FieldOfView * 0.5;
        }
    }
}