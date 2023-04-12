using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CelestialBody
{
    public int Id = -1;
    public int CenterOfMotionId = -1;

    public CelestialBody()
    {
    }
}