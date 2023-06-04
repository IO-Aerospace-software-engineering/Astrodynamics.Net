// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

namespace IO.SDK.Net.DTO;

public struct RaDec
{
    public double RightAscencion = 0.0, Declination = 0.0, Radius = 0.0;

    public RaDec(double rightAscencion, double declination, double radius)
    {
        RightAscencion = rightAscencion;
        Declination = declination;
        Radius = radius;
    }
}