using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Iis.Interfaces.Ontology;
using Microsoft.Extensions.Logging;

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
        private readonly IExtNodeService _nodeService;

        public ExportService(
            ILogger<ExportService> logger,
            IExtNodeService nodeService)
        {
            _logger = logger;
            _nodeService = nodeService;
        }

        public async Task<byte[]> ExportNodeAsync(Guid id)
        {
            try
            {
                var node = await _nodeService.GetExtNodeByIdAsync(id);
                await using var memoryStream = new MemoryStream();
                using (var doc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    var mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document
                    {
                        Body = new Body()
                    };
                    var body = mainPart.Document.Body;
                    var container = body.AppendChild(new Run());

                    AppendNodeType(node, body);
                    AppendNodes(node.Children, body);
                }

                return memoryStream.ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export object of study to doc");
                throw;
            }
        }

        private static void AppendNodeType(IExtNode node, Body container)
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
            valueRun.AppendChild(new Text($"{node.NodeTypeTitle}"));
        }

        private static void AppendNodes(IReadOnlyCollection<IExtNode> nodes, Body container)
        {
            var childNodes = nodes
                .Where(p => !NodeTypesToExclude.Contains(p.NodeTypeName));

            var nameNode = childNodes.FirstOrDefault(p => p.NodeTypeName == NameNodeTypeName);

            if (nameNode != null)
            {
                AppendNameNode(container, nameNode);
            }

            foreach (var childNode in childNodes.Where(p => p.NodeTypeName != NameNodeTypeName))
            {
                AppendRegularNode(container, childNode);
            }
        }

        private static void AppendRegularNode(Body container, IExtNode childNode)
        {
            var headerParagraph = container.AppendChild(new Paragraph());

            var headerRun = new Run()
            {
                RunProperties = GetHeaderStyles()
            };
            headerParagraph.Append(headerRun);
            headerRun.AppendChild(new Text($"{childNode.NodeTypeTitle}"));

            if (childNode.IsAttribute && !string.IsNullOrEmpty(childNode.AttributeValue))
            {
                var valueParagraph = container.AppendChild(new Paragraph());
                var valueRun = new Run()
                {
                    RunProperties = GetValueStyles()
                };
                valueParagraph.Append(valueRun);
                valueRun.AppendChild(new Text(FormatAttributeValue(childNode.AttributeValue)));
            }

            if (childNode.Children.Any())
            {
                AppendNodes(childNode.Children, container);
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

        private static void AppendNameNode(Body container, IExtNode nameNode)
        {
            var paragraph = container.AppendChild(new Paragraph());
            var valueRun = new Run()
            {
                RunProperties = GetValueStyles()
            };
            paragraph.Append(valueRun);
            valueRun.AppendChild(new Text($"{nameNode.AttributeValue}"));
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