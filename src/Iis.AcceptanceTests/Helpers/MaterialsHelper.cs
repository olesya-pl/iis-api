using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.GraphQL.Materials;
using Newtonsoft.Json;
using NPOI.XWPF.UserModel;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Xunit;

namespace AcceptanceTests.Helpers
{
    public class MaterialsHelper
    {
        private static (long, byte[]) GenerateDocxMaterial(string fileName, string content)
        {
            XWPFDocument doc = new XWPFDocument();
            var p = doc.CreateParagraph();
            var run = p.CreateRun();
            run.SetText(content);
            using var ms = new MemoryStream();

            using var fs1 = new FileStream(fileName + ".docx", FileMode.Create);

            doc.Write(fs1);
            doc.Close();
            using var fs = new FileStream(fileName + ".docx", FileMode.Open);

            fs.Position = 0;
            var size = fs.Length;
            fs.CopyTo(ms);

            ms.Position = 0;
            var bytes = ms.ToArray();
            return (size, bytes);
        }

        private static MaterialInput Create(long fileSize,
            Guid? parentMaterialId,
            string content,
            string sourceReliability,
            string reliability,
            string metaData,
            string loadedBy,
            int accessLevel = 0
            )
        {
            var data = new List<Data>
            {
                new Data {Type = "createdDate", Text = DateTime.UtcNow.ToString()},
                new Data {Type = "modifiedDate", Text = DateTime.UtcNow.ToString()},
                new Data {Type = "size", Text = fileSize.ToString("D")},
                new Data {Type = "fileName", Text = content+".docx"},
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
                //Objects = processingResult.Objects,
                //Tags = processingResult.Tags,
                //States = processingResult.States,
                //Code = processingResult.Code,
                From = "contour.doc",
                LoadedBy = loadedBy,
                //DocumentId = processingResult.DocumentId,
                CreationDate = DateTime.UtcNow,
                //Coordinates = processingResult.Coordinates,
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
            File.Delete(fileName + ".docx");
            var fileResult = JsonConvert.DeserializeObject<CreateMaterialResponse>(await response.Content.ReadAsStringAsync());
            return fileResult;
        }

        public static async Task UploadDocxMaterial(MaterialModel materialModel)
        {
            var material = MaterialsHelper.GenerateDocxMaterial(materialModel.FileName, materialModel.Content);
            var materialInput = Create(material.Item1,
                null, materialModel.Content,
                materialModel.SourceReliabilityText,
                materialModel.ReliabilityText,
                materialModel.MetaData,
                materialModel.LoadedBy,
                materialModel.AccessLevel);
            await AddMaterialAsync(materialInput, materialModel.FileName + ".docx", material.Item2, CancellationToken.None);
        }
    }
}
