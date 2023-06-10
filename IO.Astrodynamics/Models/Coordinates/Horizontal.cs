namespace IO.Astrodynamics.Models.Coordinates
{
    public readonly record struct Horizontal
    {
        public double Azimuth { get; }
        public double Elevation { get; }
        public double Altitude { get; }


        public Horizontal(double azimuth, double elevation, double altitude)
        {
            Azimuth = azimuth;
            Elevation = elevation;
            Altitude = altitude;
        }
    }
}
