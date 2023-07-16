// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Body.Spacecraft;

public class SpacecraftSummary
{
    public SpacecraftSummary(Spacecraft spacecraft, Window? maneuverWindow, double fuelConsumption)
    {
        ManeuverWindow = maneuverWindow;
        FuelConsumption = fuelConsumption;
        Spacecraft = spacecraft;
    }

    public Spacecraft Spacecraft { get; }
    public Window? ManeuverWindow { get; }
    public double FuelConsumption { get; }
}