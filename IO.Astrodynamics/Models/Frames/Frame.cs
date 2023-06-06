using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;


namespace IO.Astrodynamics.Models.Frames;

public class Frame
{
    public string Name { get; private set; }
    public int Length => _toICRFCollection.Count;

    private readonly SortedDictionary<DateTime, StateOrientation> _toICRFCollection;
    private readonly SortedDictionary<DateTime, StateOrientation> _fromICRFCollection;

    public static readonly Frame ICRF = new Frame("ICRF", new StateOrientation(new Math.Quaternion(1.0, new Math.Vector3()), new Math.Vector3(), new DateTime(2000, 01, 01, 12, 0, 0), null));
    public static readonly Frame ECLIPTIC = new Frame("ECLIPJ2000", new StateOrientation(new Math.Quaternion(0.97915322, new Math.Vector3(0.20312304, 0.0, 0.0)), new Math.Vector3(), new DateTime(2000, 01, 01, 12, 0, 0), ICRF));

    public Frame(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Frame must have a name");
        }

        Name = name;
        _toICRFCollection = new SortedDictionary<DateTime, StateOrientation>();
        _fromICRFCollection = new SortedDictionary<DateTime, StateOrientation>();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="toICRFdata"></param>
    public Frame(string name, params StateOrientation[] toICRFdata)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Frame must have a name");
        }

        Name = name;
        _toICRFCollection = new SortedDictionary<DateTime, StateOrientation>(toICRFdata.ToDictionary(x => x.Epoch));
        _fromICRFCollection = new SortedDictionary<DateTime, StateOrientation>(toICRFdata.Select(x => new StateOrientation(x.Orientation.Conjugate(), x.AngularVelocity.Inverse(), x.Epoch, x.Frame)).ToDictionary(x => x.Epoch));
    }

    public StateOrientation ToICRF(DateTime epoch)
    {
        var latestSO = _toICRFCollection.Where(x => x.Key <= epoch).LastOrDefault().Value;

        if (latestSO == null)
        {
            latestSO = _toICRFCollection.First().Value;
        }

        var deltaT = epoch - latestSO.Epoch;

        var rotation = latestSO.AngularVelocity * deltaT.TotalSeconds;

        var orientation = latestSO.Orientation;
        if (rotation != Math.Vector3.Zero)
        {
            //Conjugate to match JPL quaternion format
            orientation *= (new Math.Quaternion(rotation.Normalize(), rotation.Magnitude())).Conjugate();
        }

        return new StateOrientation(orientation, latestSO.AngularVelocity, epoch, latestSO.Frame);
    }

    public StateOrientation FromICRF(DateTime epoch)
    {
        var latestSO = _fromICRFCollection.Where(x => x.Key <= epoch).LastOrDefault().Value;

        if (latestSO == null)
        {
            latestSO = _fromICRFCollection.First().Value;
        }

        var deltaT = epoch - latestSO.Epoch;

        var rotation = latestSO.AngularVelocity * deltaT.TotalSeconds;

        var orientation = latestSO.Orientation;
        if (rotation != Math.Vector3.Zero)
        {
            orientation *= new Math.Quaternion(rotation.Normalize(), rotation.Magnitude()).Conjugate();
        }

        return new StateOrientation(orientation, latestSO.AngularVelocity, epoch, latestSO.Frame);
    }

    public void AddStateOrientationFromICRF(StateOrientation fromICRFOrientation)
    {
        if (fromICRFOrientation is null)
        {
            throw new ArgumentNullException(nameof(fromICRFOrientation));
        }

        if (fromICRFOrientation.Frame != ICRF)
        {
            throw new ArgumentException("State orientation must be relative to ICRF frame");
        }

        if (_fromICRFCollection.ContainsKey(fromICRFOrientation.Epoch)) return;
        
        _fromICRFCollection.Add(fromICRFOrientation.Epoch, fromICRFOrientation);
        _toICRFCollection.Add(fromICRFOrientation.Epoch,
            new StateOrientation(fromICRFOrientation.Orientation.Conjugate(),
                fromICRFOrientation.AngularVelocity.Inverse(), fromICRFOrientation.Epoch,
                fromICRFOrientation.Frame));
    }

    public StateOrientation ToFrame(Frame frame, DateTime epoch)
    {
        var from = frame.FromICRF(epoch);
        var to = ToICRF(epoch);
        return new StateOrientation(from.Orientation * to.Orientation, from.AngularVelocity + to.AngularVelocity, epoch, frame);
    }

    public override string ToString()
    {
        return Name;
    }
}