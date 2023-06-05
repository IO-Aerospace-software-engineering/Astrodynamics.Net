using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Users
{
    public class Group : Entity
    {
        public string Name { get; private set; }
        private List<User> _users = new List<User>();
        public IReadOnlyCollection<User> Users => _users;

        public Group(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Add user to group
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(User user)
        {
            if (!_users.Contains(user))
            {
                _users.Add(user);
                user.AddToGroup(this);
            }
        }        
    }
}