using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct FuelTank
{
    public int Id;
    public string SerialNumber;
    public double Capacity;
    public double Quantity;

    public FuelTank(int id, double capacity, double quantity, string serialNumber)
    {
        Id = id;
        Capacity = capacity;
        Quantity = quantity;
        SerialNumber = serialNumber;
    }
}