using IO.Astrodynamics.Models.Users;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Users
{
    public class GroupTests
    {
        [Fact]
        public void Create()
        {
            User user = new User("Joe");
            Group group = new Group("Admin");
            group.AddUser(user);

            Assert.Equal("Admin", group.Name);
            Assert.Equal(1, group.Users.Count);
        }
    }
}