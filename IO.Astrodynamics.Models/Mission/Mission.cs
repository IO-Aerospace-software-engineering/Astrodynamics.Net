using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Mission
{
    public class Mission : Entity
    {
        public Mission(string name, int id = default) : base(id)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}