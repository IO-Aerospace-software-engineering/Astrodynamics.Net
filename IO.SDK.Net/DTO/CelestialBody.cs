using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CelestialBody
{
    public int Id;
    public int CenterOfMotionId;

    public CelestialBody() : this(-1, -1)
    {
    }

    public CelestialBody(int id, int centerOfMotionId)
    {
        Id = id;
        CenterOfMotionId = centerOfMotionId;
    }
}