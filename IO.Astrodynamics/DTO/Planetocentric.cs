// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Planetocentric
{
    public double longitude, latitude, radius;

    public Planetocentric(double longitude, double latitude, double radius)
    {
        this.longitude = longitude;
        this.latitude = latitude;
        this.radius = radius;
    }
}