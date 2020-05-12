namespace AcceptanceTests.Environment
{
    public class LoginResponse
    {
        public UserLogin Login { get; set; }

        public class UserLogin
        {
            public string Token { get; set; }
        }
    }
}