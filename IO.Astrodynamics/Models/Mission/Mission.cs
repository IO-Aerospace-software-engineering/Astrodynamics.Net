

namespace IO.Astrodynamics.Models.Mission
{
    public class Mission 
    {
        public Mission(string name) 
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}