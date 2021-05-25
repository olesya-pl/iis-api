using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Iis.Services.Contracts.Dtos;
using Iis.Domain.Materials;

namespace IIS.Core
{
    public interface IGsmTranscriber
    {
        Task<JObject> TranscribeAsync(File file, CancellationToken cancellationToken = default);
    }

    public class GsmTranscriber : IGsmTranscriber
    {
        private readonly string _gsmWorkerUrl;
        private readonly HttpClient _httpClient = new HttpClient();

        public GsmTranscriber(string gsmWorkerUrl)
        {
            _gsmWorkerUrl = gsmWorkerUrl;
        }

        public async Task<JObject> TranscribeAsync(File file, CancellationToken cancellationToken = default)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("1"), "trim");
            content.Add(new StringContent("4"), "dur");
            content.Add(new ByteArrayContent(file.ContentBytes, 0, file.ContentBytes.Length), "file", "file.wav");
            var response = await _httpClient.PostAsync(_gsmWorkerUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var mlResponse = JObject.Parse(responseString);
            return mlResponse;
        }
    }
}
