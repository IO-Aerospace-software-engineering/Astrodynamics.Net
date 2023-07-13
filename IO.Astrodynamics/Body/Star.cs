using System;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Body;

public class Star:CelestialBody
{
    public Star(int catalogNumber, string spectralType, double visualMagnitude, double parallax, Equatorial equatorialCoordinatesAtEpoch, double declinationProperMotion,
        double rightAscensionProperMotion, double declinationSigma, double rightAscensionSigma, double declinationSigmaProperMotion, double rightAscensionSigmaProperMotion,
        DateTime epoch):base()
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
        Distance = 1 / Parallax;
    }

    public int CatalogNumber { get; }
    public string SpectralType { get; }
    public double VisualMagnitude { get; }

    public double Parallax { get; }
    
    /// <summary>
    /// Distance in parsec
    /// </summary>
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
        return new Equatorial(EquatorialCoordinatesAtEpoch.Declination + dt * DeclinationProperMotion,
            EquatorialCoordinatesAtEpoch.RightAscencion + dt * RightAscensionProperMotion, Distance);
    }

    public double GetRightAscensionSigma(DateTime epoch)
    {
        var dt = (epoch.ToJulianDate() - Epoch.ToJulianDate()) / DateTimeExtension.JULIAN_YEAR;
        return System.Math.Sqrt(System.Math.Pow(RightAscensionSigma, 2) + System.Math.Pow(dt * RightAscensionSigmaProperMotion, 2));
    }

    public double GetDeclinationSigma(DateTime epoch)
    {
        var dt = (epoch.ToJulianDate() - Epoch.ToJulianDate()) / DateTimeExtension.JULIAN_YEAR;
        return System.Math.Sqrt(System.Math.Pow(DeclinationSigma, 2) + System.Math.Pow(dt * DeclinationSigmaProperMotion, 2));
    }
}