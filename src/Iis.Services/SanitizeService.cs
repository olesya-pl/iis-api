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
        private const string Base64Image = "base64image";

        public SanitizeService()
        {
            _sanitizers = new List<(Func<string, bool> IsSuitable, Func<string, string> Sanitize)>
            {
                (IsSuitable: IsE2ELoginRequest, Sanitize: SanitizeE2ELoginRequestBody),
                (IsSuitable: IsLoginRequest, Sanitize: SanitizeLoginRequestBody),
                (IsSuitable: IsLoginResponse, Sanitize: SanitizeLoginResponseBody),
                (IsSuitable: IsContainsBase64Request, Sanitize: RemoveBase64ImageFromRequest),
                (IsSuitable: IsTooLong, Sanitize: TrimBody)
            };
        }

        public string SanitizeBody(string body)
        {
            foreach (var sanitizer in _sanitizers.Where(_ => _.IsSuitable(body)))
            {
                body = sanitizer.Sanitizer(body);
            }
            return body;
        }
        private static bool IsE2ELoginRequest(string body) => body.Contains("mutation") && body.Contains("Login");
        private static string SanitizeE2ELoginRequestBody(string body) => ReplaceFields(body, ReplaceTo, "u", "p");
        private static bool IsLoginRequest(string body) => body.Contains("mutation") && body.Contains("loginOnApi");
        private static string SanitizeLoginRequestBody(string body) => ReplaceFields(body, ReplaceTo, "username", "password");
        private static bool IsLoginResponse(string body) => body.Contains("login") && body.Contains("token");
        private static string SanitizeLoginResponseBody(string body) => ReplaceFields(body, ReplaceTo, "token");
        private static bool IsContainsBase64Request(string body) => body.Contains("searchByImageInput");
        private static string RemoveBase64ImageFromRequest(string body) => ReplaceFields(body, Base64Image, "content");
        private static bool IsTooLong(string body) => body.Length > 2000;
        private static string TrimBody(string body) => body.Substring(0, 2000) + " (...)Trimmed";

        private static string ReplaceFields(string body, string replaceTo, params string[] fields)
        {
            var fieldsInRegex = $"{string.Join("|", fields.Select(x => $"\"{x}\""))}";
            var regex = string.Format(RegexTemplate, fieldsInRegex);
            return Regex.Replace(body, regex, $"$1:\"{replaceTo}\"");
        }
    }
}