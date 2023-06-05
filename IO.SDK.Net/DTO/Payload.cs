// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Payload
{
    public string SerialNumber;
    public string Name;
    public double Mass;

    public Payload(string serialNumber, string name, double mass)
    {
        SerialNumber = serialNumber;
        Name = name;
        Mass = mass;
    }
}