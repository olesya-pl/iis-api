using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Iis.Interfaces.Repository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Iis.Api.Export
{
    public class ExportService
    {
        private const int FontSize = 28;
        private const string NameNodeTypeName = "name";
        private const string HeaderFontColor = "#757575";
        private static readonly string[] NodeTypesToExclude =
        {
            "code", "attachment", "photo"
        };

        private readonly ILogger<ExportService> _logger;
        private readonly INodeRepository _nodeRepository;

        public ExportService(
            ILogger<ExportService> logger,
            INodeRepository nodeRepository)
        {
            _logger = logger;
            _nodeRepository = nodeRepository;
        }

        public async Task<byte[]> ExportNodeAsync(Guid id)
        {
            try
            {
                var node = await _nodeRepository.GetJsonNodeByIdAsync(id);
                await using var memoryStream = new MemoryStream();
                using (var doc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    var mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document
                    {
                        Body = new Body()
                    };
                    var body = mainPart.Document.Body;

                    AppendNodeType(node, body);
                    AppendNodes(node, body);

                    doc.Close();
                }

                return memoryStream.ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export object of study to doc");
                throw;
            }
        }

        private static void AppendNodeType(JObject node, Body container)
        {
            var headerParagraph = container.AppendChild(new Paragraph());
            var headerRun = new Run()
            {
                RunProperties = GetHeaderStyles()
            };
            headerParagraph.Append(headerRun);
            headerRun.AppendChild(new Text("Тип"));

            var valueParagraph = container.AppendChild(new Paragraph());
            var valueRun = new Run()
            {
                RunProperties = GetValueStyles()
            };
            valueParagraph.Append(valueRun);
            valueRun.AppendChild(new Text($"{node["NodeTypeTitle"]}"));
        }

        private static void AppendNodes(JObject nodes, Body container)
        {
            var nameNode = nodes.Properties().FirstOrDefault(p => p.Name == NameNodeTypeName);
            if (nameNode != null)
            {
                AppendNameNode(container, nameNode);
            }

            foreach (var childNode in ((IEnumerable<KeyValuePair<string, JToken>>)nodes)
                .Where(p => !NodeTypesToExclude.Contains(p.Key)))
            {
                AppendRegularNode(container, childNode.Key, childNode.Value);
            }
        }

        private static void AppendRegularNode(Body container, string name, JToken childNode)
        {
            var headerParagraph = container.AppendChild(new Paragraph());

            var headerRun = new Run()
            {
                RunProperties = GetHeaderStyles()
            };
            headerParagraph.Append(headerRun);

            headerRun.AppendChild(new Text(name));

            if (childNode is JObject)
            {
                AppendNodes(childNode as JObject, container);
            }
            else
            {
                var valueParagraph = container.AppendChild(new Paragraph());
                var valueRun = new Run()
                {
                    RunProperties = GetValueStyles()
                };
                valueParagraph.Append(valueRun);
                valueRun.AppendChild(new Text(FormatAttributeValue(childNode.ToString())));
            }
        }

        private static string FormatAttributeValue(string input)
        {
            if (DateTime.TryParse(input, out var _))
            {
                return FormatDate(input);
            };
            return input;
        }

        private static string FormatDate(string input)
        {
            var splitted = input.Split('T');
            if (splitted.Length == 2)
            {
                return splitted[0];
            }
            else
            {
                return input;
            }
        }

        private static void AppendNameNode(Body container, JProperty nameNode)
        {
            var paragraph = container.AppendChild(new Paragraph());
            var valueRun = new Run()
            {
                RunProperties = GetValueStyles()
            };
            paragraph.Append(valueRun);
            valueRun.AppendChild(new Text($"{nameNode.Value}"));
        }

        private static RunProperties GetValueStyles()
        {
            return new RunProperties()
            {
                FontSize = new FontSize()
                {
                    Val = $"{FontSize}"
                }
            };
        }

        private static RunProperties GetHeaderStyles()
        {
            return new RunProperties()
            {
                FontSize = new FontSize()
                {
                    Val = $"{FontSize}"
                },
                Color = new Color
                {
                    Val = HeaderFontColor
                }
            };
        }
    }
}