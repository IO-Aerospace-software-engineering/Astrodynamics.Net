// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.ComponentModel;

namespace IO.Astrodynamics;

public enum InertialFrame
{
    [Description("J2000")] ICRF,
    [Description("ECLIPJ2000")] Ecliptic,
    [Description("GALACTIC")] Galactic
}