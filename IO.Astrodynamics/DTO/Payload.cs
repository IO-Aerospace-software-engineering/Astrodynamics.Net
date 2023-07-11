// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Payload
{
    public readonly string SerialNumber;
    public readonly string Name;
    public readonly double Mass;

    public Payload(string serialNumber, string name, double mass)
    {
        SerialNumber = serialNumber;
        Name = name;
        Mass = mass;
    }
}