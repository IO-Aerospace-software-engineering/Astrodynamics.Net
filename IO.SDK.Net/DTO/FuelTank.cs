using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct FuelTank
{
    public int Id;
    public double Capacity;
    public double Quantity;
}