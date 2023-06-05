using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Users
{
    public class User : Entity
    {
        public string Name { get; private set; }

        private List<Group> _groups = new List<Group>();
        public IReadOnlyCollection<Group> Groups => _groups;

        public User(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("User requires a name");
            }
            Name = name;
        }

        /// <summary>
        /// Associate this user to group
        /// </summary>
        /// <param name="group"></param>
        public void AddToGroup(Group group)
        {
            if (!_groups.Contains(group))
            {
                _groups.Add(group);
                group.AddUser(this);
            }

        }
    }
}