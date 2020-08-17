using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Interfaces;

namespace Iis.Services
{
    public class SanitizeService : ISanitizeService
    {
        private readonly List<(Func<string, bool> IsSuitable, Func<string, string> Sanitizer)> _sanitizers;
        private const string RegexTemplate = "({0}):\\s*(\"([^\"]+)\")";
        private const string ReplaceTo = "hidden_information";

        public SanitizeService()
        {
            _sanitizers = new List<(Func<string, bool> IsSuitable, Func<string, string> Sanitizer)>
            {
                (IsSuitable: IsE2ELoginRequest, Sanitizer: SanitizeE2ELoginRequestBody),
                (IsSuitable: IsLoginRequest, Sanitizer: SanitizeLoginRequestBody),
                (IsSuitable: IsLoginResponse, Sanitizer: SanitizeLoginResponseBody)
            };
        }

        public string SanitizeBody(string body)
        {
            var sanitizer = _sanitizers.FirstOrDefault(s => s.IsSuitable(body));
            return sanitizer.Sanitizer == null ? body : sanitizer.Sanitizer(body);
        }

        #region E2ELoginRequest

        private static bool IsE2ELoginRequest(string body) => body.Contains("mutation") && body.Contains("Login");

        private static string SanitizeE2ELoginRequestBody(string body) => ReplaceFields(body, ReplaceTo, "u", "p");

        #endregion

        #region LoginRequest

        private static bool IsLoginRequest(string body) => body.Contains("mutation") && body.Contains("loginOnApi");

        private static string SanitizeLoginRequestBody(string body) => ReplaceFields(body, ReplaceTo, "username", "password");

        #endregion

        #region LoginResponse

        private static bool IsLoginResponse(string body) => body.Contains("login") && body.Contains("token");

        private static string SanitizeLoginResponseBody(string body) => ReplaceFields(body, ReplaceTo, "token");

        #endregion


        private static string ReplaceFields(string body, string replaceTo, params string[] fields)
        {
            var fieldsInRegex = $"{string.Join("|", fields.Select(x => $"\"{x}\""))}";
            var regex = string.Format(RegexTemplate, fieldsInRegex);
            return Regex.Replace(body, regex, $"$1:\"{replaceTo}\"");
        }
    }
}