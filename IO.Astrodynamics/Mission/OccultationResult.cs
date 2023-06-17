using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Mission
{
    public class OccultationResult
    {
        public OccultationType OccultationType { get; }
        public Window Window { get; }

        public OccultationResult(in OccultationType occultationType, in Window window)
        {
            OccultationType = occultationType;
            Window = window;
        }
    }
}
