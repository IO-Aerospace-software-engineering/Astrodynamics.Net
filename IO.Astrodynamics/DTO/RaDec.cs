// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

namespace IO.Astrodynamics.DTO;

public struct RaDec
{
    public readonly double RightAscencion = 0.0;
    public readonly double Declination = 0.0;
    public readonly double Radius = 0.0;

    public RaDec(double rightAscencion, double declination, double radius)
    {
        RightAscencion = rightAscencion;
        Declination = declination;
        Radius = radius;
    }
}