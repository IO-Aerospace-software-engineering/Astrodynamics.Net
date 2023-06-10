namespace IO.Astrodynamics.Models.Body
{
    public interface INaifObject
    {
        int NaifId { get; }
        string Name { get; }
        
        //Todo add frame ?
    }
}