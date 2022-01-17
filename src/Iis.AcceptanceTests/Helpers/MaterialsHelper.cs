using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AcceptanceTests.Helpers.FileGenerators;
using IIS.Core.GraphQL.Materials;
using Newtonsoft.Json;
using NPOI.XWPF.UserModel;

namespace AcceptanceTests.Helpers
{
    public class MaterialsHelper
    {
        private static MaterialInput Create(long fileSize,
            Guid? parentMaterialId,
            string content,
            string sourceReliability,
            string reliability,
            string metaData,
            string loadedBy,
            string source = "contour.doc",
            string fileExtension = "docx",
            int accessLevel = 0
            )
        {
            var data = new Data[]
            {
                new Data {Type = "createdDate", Text = DateTime.UtcNow.ToString()},
                new Data {Type = "modifiedDate", Text = DateTime.UtcNow.ToString()},
                new Data {Type = "size", Text = fileSize.ToString("D")},
                new Data {Type = "fileName", Text = $"content.{fileExtension}"},
                new Data {Type = "originalContent", Text = content}
            };


            return new MaterialInput
            {
                ParentId = parentMaterialId,
                Metadata = metaData,
                Data = data,
                Title = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString(),
                Content = content,
                ReliabilityText = reliability,
                SourceReliabilityText = sourceReliability,
                From = source,
                LoadedBy = loadedBy,
                CreationDate = DateTime.UtcNow,
                AccessLevel = accessLevel
            };
        }

        private static async Task<CreateMaterialResponse> AddMaterialAsync(
            MaterialInput material, string fileName, byte[] fileContent,
            CancellationToken cancellation)
        {
            using var form = new MultipartFormDataContent();
            using ByteArrayContent content = new ByteArrayContent(fileContent);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(content, "file", fileName);
            form.Add(new StringContent(JsonConvert.SerializeObject(material)), "input");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(TestData.BaseApiAddress)
            };
            var response = await httpClient.PostAsync("api/Files/CreateMaterial", form, cancellation);
            response.EnsureSuccessStatusCode();
            File.Delete(fileName);
            var fileResult = JsonConvert.DeserializeObject<CreateMaterialResponse>(await response.Content.ReadAsStringAsync());
            return fileResult;
        }

        public static async Task<Guid> UploadDocxMaterial(MaterialModel materialModel)
        {
            var material = DocxGenerator.GenerateDocxMaterial(materialModel.FileName, materialModel.Content);
            var materialInput = Create(material.Item1,
                null, materialModel.Content,
                materialModel.SourceReliabilityText,
                materialModel.ReliabilityText,
                materialModel.MetaData,
                materialModel.LoadedBy,
                materialModel.From,
                "docx",
                materialModel.AccessLevel);
            var response = await AddMaterialAsync(materialInput, $"{materialModel.FileName}.docx", material.Item2, CancellationToken.None);
            return response.Id;
        }

        public static async Task<Guid> UploadMp3Material(MaterialModel materialModel)
        {
            var material = Mp3Generator.Generate();
            var materialInput = Create(material.Item1,
                null, materialModel.Content,
                materialModel.SourceReliabilityText,
                materialModel.ReliabilityText,
                materialModel.MetaData,
                materialModel.LoadedBy,
                materialModel.From,
                "mp3",
                materialModel.AccessLevel);
            var response = await AddMaterialAsync(materialInput, $"{materialModel.FileName}.mp3", material.Item2, CancellationToken.None);
            return response.Id;
        }

        internal static Task RemoveMaterialAsync(Guid id)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(TestData.BaseApiAddress)
            };
            return httpClient.GetAsync($"api/material/RemoveMaterial/{id}");
        }
    }
}
