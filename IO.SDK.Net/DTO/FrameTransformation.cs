using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct FrameTransformation
{
    public Quaternion Rotation;
    public Vector3D AngularVelocity;
}