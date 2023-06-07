using System;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Math;

namespace IO.Astrodynamics.Models.OrbitalParameters
{
    public record class StateOrientation
    {
        public Quaternion Rotation { get; }
        public DateTime Epoch { get; }
        
        /// <summary>
        /// Frame from which the rotation is applied
        /// </summary>
        public Frames.Frame ReferenceFrame { get; }
        public Vector3 AngularVelocity { get; }

        public StateOrientation(Quaternion orientation, Vector3 angularVelocity, DateTime epoch, Frames.Frame frame)
        {
            Rotation = orientation;
            AngularVelocity = angularVelocity;
            Epoch = epoch;
            ReferenceFrame = frame;
        }

        public StateOrientation RelativeToICRF()
        {
            return RelativeTo(Frame.ICRF);
        }

        public StateOrientation RelativeTo(Frame frame)
        {
            if (ReferenceFrame == frame)
            {
                return this;
            }

            return new StateOrientation(Rotation * ReferenceFrame.ToFrame(frame, Epoch).Rotation,
                AngularVelocity - Frames.Frame.ICRF.ToFrame(frame, Epoch).AngularVelocity, Epoch, frame);
        }
    }
}