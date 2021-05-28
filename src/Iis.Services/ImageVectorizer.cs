using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Services.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Iis.Services
{
    public class ImageVectorizer : IImageVectorizer
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _imageVectorizerUrl;

        public ImageVectorizer(IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _imageVectorizerUrl = configuration.GetValue<string>("imageVectorizerUrl");
        }

        public async Task<IReadOnlyCollection<decimal[]>> VectorizeImage(byte[] fileContent, string fileName)
        {
            using var form = new MultipartFormDataContent();
            using var content = new ByteArrayContent(fileContent);

            form.Add(content, "file", fileName);
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(_imageVectorizerUrl, form);
            response.EnsureSuccessStatusCode();
            var contentJson = await response.Content.ReadAsStringAsync();
            return FaceAPIResponseParser.GetFaceVectorList(contentJson);
        }
    }
}
