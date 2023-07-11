// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct EngineDTO
{
    public readonly int Id;
    public readonly string SerialNumber;
    public readonly string FuelTankSerialNumber;
    public readonly string Name;
    public readonly double Isp;
    public readonly double FuelFlow;

    public EngineDTO(int id, string name, double fuelFlow, string serialNumber, string fuelTankSerialNumber, double isp)
    {
        Id = id;
        Name = name;
        FuelFlow = fuelFlow;
        SerialNumber = serialNumber;
        FuelTankSerialNumber = fuelTankSerialNumber;
        Isp = isp;
    }
}