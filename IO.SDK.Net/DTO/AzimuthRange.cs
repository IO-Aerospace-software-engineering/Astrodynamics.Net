// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct AzimuthRange
{
    public double Start, End;

    public AzimuthRange(double start, double end)
    {
        Start = start;
        End = end;
    }
}