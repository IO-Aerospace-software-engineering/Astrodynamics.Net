using System;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Frames;

public class Frame
{
    public string Name { get; }

    private readonly API _api;

    public static readonly Frame ICRF = new Frame("J2000");
    public static readonly Frame ECLIPTIC = new Frame("ECLIPJ2000");

    public Frame(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Frame must have a name");
        }

        _api = API.Instance;
        Name = name;
    }

    public StateOrientation ToFrame(Frame frame, DateTime epoch)
    {
        return _api.TransformFrame(this, frame, epoch);
    }

    public override string ToString()
    {
        return Name;
    }
}