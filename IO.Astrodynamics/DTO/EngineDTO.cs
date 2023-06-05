// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct EngineDTO
{
    public int Id;
    public string SerialNumber;
    public string FuelTankSerialNumber;
    public string Name;
    public double Isp;
    public double FuelFlow;

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