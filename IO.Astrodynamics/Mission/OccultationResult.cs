using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Mission
{
    public class OccultationResult
    {
        public OccultationType OccultationType { get; private set; }
        public Window Window { get; private set; }

        public OccultationResult(in OccultationType occultationType, in Window window)
        {
            OccultationType = occultationType;
            Window = window;
        }
    }
}
