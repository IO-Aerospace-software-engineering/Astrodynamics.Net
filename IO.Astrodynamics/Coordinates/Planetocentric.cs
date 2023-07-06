namespace IO.Astrodynamics.Coordinates
{
    public readonly record struct Planetocentric
    {
        public double Longitude { get; }
        public double Latitude { get; }
        public double Altitude { get; }

        public Planetocentric(double longitude, double latitude, double altitude)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
        }
    }
}
