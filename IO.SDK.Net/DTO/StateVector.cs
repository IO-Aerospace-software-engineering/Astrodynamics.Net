using System.Net.Http;
using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct StateVector
{
    public double Epoch;
    public Vector3D Position;
    public Vector3D Velocity;
    public CelestialBody CenterOfMotion;
    public string Frame;
}