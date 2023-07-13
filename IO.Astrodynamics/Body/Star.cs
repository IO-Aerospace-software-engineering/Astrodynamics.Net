using System;
using System.Collections.Generic;
using System.IO;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Body;

public class Star : CelestialBody
{
    public Star(int catalogNumber, string name, double mass, string spectralType, double visualMagnitude, double parallax, Equatorial equatorialCoordinatesAtEpoch,
        double declinationProperMotion, double rightAscensionProperMotion, double declinationSigma, double rightAscensionSigma, double declinationSigmaProperMotion,
        double rightAscensionSigmaProperMotion, DateTime epoch) : base((int)1E+09 + catalogNumber, name, mass)
    {
        CatalogNumber = catalogNumber;
        SpectralType = spectralType ?? throw new ArgumentNullException(nameof(spectralType));
        VisualMagnitude = visualMagnitude;
        Epoch = epoch;
        Parallax = parallax;
        EquatorialCoordinatesAtEpoch = equatorialCoordinatesAtEpoch;
        RightAscensionProperMotion = rightAscensionProperMotion;
        DeclinationProperMotion = declinationProperMotion;
        RightAscensionSigma = rightAscensionSigma;
        DeclinationSigma = declinationSigma;
        RightAscensionSigmaProperMotion = rightAscensionSigmaProperMotion;
        DeclinationSigmaProperMotion = declinationSigmaProperMotion;
        Distance = (1 / Parallax) * Constants.Parsec2Meters;
    }

    public int CatalogNumber { get; }
    public string SpectralType { get; }
    public double VisualMagnitude { get; }

    /// <summary>
    /// ArcSec
    /// </summary>
    public double Parallax { get; }

    public double Distance { get; }

    public DateTime Epoch { get; }
    public Equatorial EquatorialCoordinatesAtEpoch { get; }

    public double RightAscensionProperMotion { get; }
    public double DeclinationProperMotion { get; }

    public double RightAscensionSigma { get; }
    public double DeclinationSigma { get; }

    public double RightAscensionSigmaProperMotion { get; }
    public double DeclinationSigmaProperMotion { get; }

    public Equatorial GetEquatorialCoordinates(DateTime epoch)
    {
        var dt = (epoch.ToJulianDate() - Epoch.ToJulianDate()) / DateTimeExtension.JULIAN_YEAR;
        var dec = (EquatorialCoordinatesAtEpoch.Declination + dt * DeclinationProperMotion) % Constants.PI2;

        var ra = (EquatorialCoordinatesAtEpoch.RightAscension + dt * RightAscensionProperMotion) % Constants._2PI;
        if (ra < 0.0)
        {
            ra += Constants._2PI;
        }

        return new Equatorial(dec, ra, Distance);
    }

    public double GetRightAscensionSigma(DateTime epoch)
    {
        var dt = (epoch.ToJulianDate() - Epoch.ToJulianDate()) / DateTimeExtension.JULIAN_YEAR;
        return System.Math.Sqrt(System.Math.Pow(RightAscensionSigma, 2) + System.Math.Pow(dt * RightAscensionSigmaProperMotion, 2)) % Constants._2PI;
    }

    public double GetDeclinationSigma(DateTime epoch)
    {
        var dt = (epoch.ToJulianDate() - Epoch.ToJulianDate()) / DateTimeExtension.JULIAN_YEAR;
        return System.Math.Sqrt(System.Math.Pow(DeclinationSigma, 2) + System.Math.Pow(dt * DeclinationSigmaProperMotion, 2)) % Constants.PI2;
    }

    public void Propagate(Window timeWindow, TimeSpan stepSize)
    {
        var path = new FileInfo($"Data/Stars/star{NaifId}.spk");
        List<StateVector> svs = new List<StateVector>();
        for (DateTime epoch = timeWindow.StartDate; epoch <= timeWindow.EndDate; epoch += stepSize)
        {
            var position = GetEquatorialCoordinates(epoch).ToCartesian();
            svs.Add(new StateVector(position, Vector3.Zero, new Barycenter(0), epoch, Frame.ICRF));
        }

        if (API.Instance.WriteEphemeris(path, this, svs))
        {
            API.Instance.LoadKernels(path);
        }
    }
}