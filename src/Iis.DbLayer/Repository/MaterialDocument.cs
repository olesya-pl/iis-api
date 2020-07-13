using System;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repository
{
    internal class MaterialDocument
    {
        public Guid Id { get; set; }
        public Guid? FileId { get; set; }
        public JObject Metadata { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string CreatedDate { get; set; }
        public string Content { get; set; }
        public MaterialSign Importance { get; set; }
        public MaterialSign Reliability { get; set; }
        public MaterialSign Relevance { get; set; }
        public MaterialSign Completeness { get; set; }
        public MaterialSign SourceReliability { get; set; }
        public MaterialSign ProcessedStatus { get; set; }
        public MaterialSign SessionPriority { get; set; }
        public Data[] Data { get; set; }
        public JObject[] Transcriptions { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public string[] Objects { get; set; } = new string[0];
        public string[] Tags { get; set; } = new string[0];
        public string[] States { get; set; } = new string[0];
        public MaterialDocument[] Children { get; set; }
        public int MlHandlersCount { get; set; }
        public int ProcessedMlHandlersCount { get; set; }
        public Guid[] NodeIds { get; set; }
        public Assignee Assignee { get; set; }
    }

    internal class MaterialSign
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }

    internal class Data
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }

    internal class Assignee
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }

    }
}
