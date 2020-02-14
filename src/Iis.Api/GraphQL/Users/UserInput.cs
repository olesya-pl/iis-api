using System.ComponentModel.DataAnnotations;
using HotChocolate;

namespace IIS.Core.GraphQL.Users
{
    public class UserInput
    {
        public string Name { get; set; }

        public string Username { get; set; }

        public bool? IsBlocked { get; set; }

        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must have at least 6 symbols, maximum length is 100")]
        public string Password { get; set; }
    }
}
