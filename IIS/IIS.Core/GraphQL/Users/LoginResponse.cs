namespace IIS.Core.GraphQL.Users
{
    public class LoginResponse
    {
        public User   CurrentUser { get; set; }
        public string Token       { get; set; }
    }
}
