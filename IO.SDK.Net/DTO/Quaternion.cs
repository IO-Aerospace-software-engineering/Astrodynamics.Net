using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Quaternion
{
    public double W;
    public double X;
    public double Y;
    public double Z;
}