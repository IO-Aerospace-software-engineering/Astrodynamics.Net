// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct FuelTank
{
    public readonly int Id;
    public readonly string SerialNumber;
    public readonly double Capacity;
    public readonly double Quantity;

    public FuelTank(int id, double capacity, double quantity, string serialNumber)
    {
        Id = id;
        Capacity = capacity;
        Quantity = quantity;
        SerialNumber = serialNumber;
    }
}