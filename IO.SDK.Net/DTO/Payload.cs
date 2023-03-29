using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Payload
{
    public int SerialNumber;
    public string Name;
    public double Mass;
}