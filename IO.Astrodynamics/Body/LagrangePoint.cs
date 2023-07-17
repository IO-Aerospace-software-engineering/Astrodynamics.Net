using System;
using IO.Astrodynamics.Frames;

namespace IO.Astrodynamics.Body;

public class LagrangePoint : Body
{
    public LagrangePoint(int naifId, Frame frame, DateTime epoch) : base(naifId, frame, epoch)
    {
    }
}