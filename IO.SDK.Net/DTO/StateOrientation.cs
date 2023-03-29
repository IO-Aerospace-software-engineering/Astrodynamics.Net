using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct StateOrientation
{
    public Quaternion Orientation;
    public Vector3D AngularVelocity;
    public double Epoch;
    public string Frame;
}