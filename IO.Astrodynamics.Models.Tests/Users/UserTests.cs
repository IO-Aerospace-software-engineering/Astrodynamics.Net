using IO.Astrodynamics.Models.Users;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Users
{
    public class UserTests
    {
        [Fact]
        public void Create()
        {
            User user = new User("Joe");
            Group group = new Group("Admin");
            user.AddToGroup(group);

            Assert.Equal("Joe", user.Name);
            Assert.Equal(1, user.Groups.Count);
        }
    }
}