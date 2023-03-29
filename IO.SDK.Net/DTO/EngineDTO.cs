using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct EngineDTO
{
    public int Id;
    public double Isp;
    public double Fuelflow;
}