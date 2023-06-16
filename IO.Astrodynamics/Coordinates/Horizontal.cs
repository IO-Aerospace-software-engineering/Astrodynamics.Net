namespace IO.Astrodynamics.Coordinates
{
    public readonly record struct Horizontal
    {
        public double Azimuth { get; }
        public double Elevation { get; }
        public double Range { get; }


        public Horizontal(double azimuth, double elevation, double range)
        {
            Azimuth = azimuth;
            Elevation = elevation;
            Range = range;
        }
    }
}
