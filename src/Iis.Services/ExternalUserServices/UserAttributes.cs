namespace Iis.Services.ExternalUserServices
{
    public static class UserAttributes
    {
        public const string UserName = "sAMAccountName";
        public const string GivenName = "givenname";
        public const string MiddleName = "middlename";
        public const string Cn = "cn";
        public const string MemberOf = "memberof";

        public static readonly string[] All = new string[] { UserName, GivenName, MiddleName, Cn, MemberOf };
    }
}