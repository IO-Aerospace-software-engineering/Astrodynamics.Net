// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

namespace IO.Astrodynamics.Body;

public class GeopotentialModelParameters
{
    public GeopotentialModelParameters(string geopotentialModelPath, ushort geopotentialDegree = 60)
    {
        GeopotentialModelPath = geopotentialModelPath;
        GeopotentialDegree = geopotentialDegree;
    }

    public string GeopotentialModelPath { get; }
    public ushort GeopotentialDegree { get; }
}