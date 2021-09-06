﻿using System;
using System.Text;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic
{
    internal class ElasticLogUtils
    {
        public ElasticLogUtils()
        {
        }

        public static (LogLevel LogLevel, string Message) PrepareLog(string request, StringResponse response)
        {
            
            var sb = new StringBuilder($"Request {response.HttpMethod} with path {response.Uri} and request {request} completed. Success:{response.Success} ");
            try
            {
                var json = JObject.Parse(response.Body);
                if (json["took"] != null)
                {
                    sb.Append($"Took:{json["took"]}. ");
                }
                if (json["timed_out"] != null)
                {
                    sb.Append($"Timed out:{json["timed_out"]}.");
                }
            }
            catch (Exception) { }
            if (response.Success)
            {
                return (LogLevel.Information, sb.ToString());
            }
            else
            {
                response.TryGetServerError(out var error);
                if (error?.Error?.Reason != null)
                {
                    sb.Append($"Error {error.Error.Reason}. ");
                }
                return (LogLevel.Error, sb.ToString());
            }
        }
    }
}
