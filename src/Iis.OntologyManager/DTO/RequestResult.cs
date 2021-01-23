using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.DTO
{
    public class RequestResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public Uri RequestUrl { get; private set; }
        private RequestResult() { }

        public static RequestResult Success(string message, Uri requestUrl)
        {
            return new RequestResult
            {
                IsSuccess = true,
                Message = message,
                RequestUrl = requestUrl
            };
        }
        public static RequestResult Fail(string errorMessage, Uri requestUrl)
        {
            return new RequestResult
            {
                IsSuccess = false,
                Message = errorMessage,
                RequestUrl = requestUrl
            };
        }
    }
}
