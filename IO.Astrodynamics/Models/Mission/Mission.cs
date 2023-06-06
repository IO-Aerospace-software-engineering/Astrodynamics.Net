

namespace IO.Astrodynamics.Models.Mission
{
    public class Mission 
    {
        public Mission(string name, int id = default) 
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}