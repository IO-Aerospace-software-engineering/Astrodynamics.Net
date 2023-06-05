// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

namespace IO.Astrodynamics.SolarSystemObjects;

public static class Barycenters
{
    public static SolarSytemObject SOLAR_SYSTEM_BARYCENTER = new(0, "SOLAR SYSTEM BARYCENTER", string.Empty);
    public static SolarSytemObject MERCURY_BARYCENTER = new(1, "MERCURY BARYCENTER", string.Empty);
    public static SolarSytemObject VENUS_BARYCENTER = new(2, "VENUS BARYCENTER", string.Empty);
    public static SolarSytemObject EARTH_BARYCENTER = new(3, "EARTH MOON BARYCENTER", string.Empty);
    public static SolarSytemObject MARS_BARYCENTER = new(4, "MARS BARYCENTER", string.Empty);
    public static SolarSytemObject JUPITER_BARYCENTER = new(5, "JUPITER BARYCENTER", string.Empty);
    public static SolarSytemObject SATURN_BARYCENTER = new(6, "SATURN BARYCENTER", string.Empty);
    public static SolarSytemObject URANUS_BARYCENTER = new(7, "URANUS BARYCENTER", string.Empty);
    public static SolarSytemObject NEPTUNE_BARYCENTER = new(8, "NEPTUNE BARYCENTER", string.Empty);
    public static SolarSytemObject PLUTO_BARYCENTER = new(9, "PLUTO BARYCENTER", string.Empty);
}