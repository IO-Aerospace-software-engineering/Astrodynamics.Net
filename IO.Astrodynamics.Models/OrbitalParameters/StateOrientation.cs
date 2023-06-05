using System;
using IO.Astrodynamics.Models.Frame;
using IO.Astrodynamics.Models.Math;

namespace IO.Astrodynamics.Models.OrbitalParameters
{
    public record class StateOrientation
    {
        public Quaternion Orientation { get; set; }
        public DateTime Epoch { get; private set; }
        public Frame.Frame Frame { get; private set; }
        public Vector3 AngularVelocity { get; private set; }

        public StateOrientation(Quaternion orientation, Vector3 angularVelocity, DateTime epoch, Frame.Frame frame)
        {
            Orientation = orientation;
            AngularVelocity = angularVelocity;
            Epoch = epoch;
            Frame = frame;
        }

        public StateOrientation RelativeToICRF()
        {
            return RelativeTo(Models.Frame.Frame.ICRF);
        }

        public StateOrientation RelativeTo(Frame.Frame frame)
        {
            if (Frame == frame)
            {
                return this;
            }

            return new StateOrientation(Orientation * Frame.ToFrame(frame, Epoch).Orientation, AngularVelocity - frame.FromICRF(Epoch).AngularVelocity, Epoch, frame);
        }
    }
}