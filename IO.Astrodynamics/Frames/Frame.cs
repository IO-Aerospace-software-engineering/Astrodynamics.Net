using System;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Frames;

public class Frame : IEquatable<Frame>
{
    public string Name { get; }

    public static readonly Frame ICRF = new Frame("J2000");
    public static readonly Frame ECLIPTIC = new Frame("ECLIPJ2000");

    public Frame(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Frame must have a name");
        }
        Name = name;
    }

    public StateOrientation ToFrame(Frame frame, DateTime epoch)
    {
        return API.Instance.TransformFrame(this, frame, epoch);
    }

    public override string ToString()
    {
        return Name;
    }
    
    public bool Equals(Frame other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Frame)obj);
    }

    public override int GetHashCode()
    {
        return (Name != null ? Name.GetHashCode() : 0);
    }

    public static bool operator ==(Frame left, Frame right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Frame left, Frame right)
    {
        return !Equals(left, right);
    }
}