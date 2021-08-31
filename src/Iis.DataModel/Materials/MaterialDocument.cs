using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.DataModel;

namespace Iis.DbLayer.Repositories
{
    public class MaterialDocument
    {
        public const int ImageVectorDimensionsCount = 128;

        public Guid Id { get; set; }
        public int AccessLevel { get; set; }
        public Guid? FileId { get; set; }
        public Guid? ParentId { get; set; }
        public JObject Metadata { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Channel { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedAt { get; set; }
        public string Content { get; set; }
        public string FileName { get; set; }
        public MaterialLoadData LoadData { get; set; }
        public MaterialSign Importance { get; set; }
        public MaterialSign Reliability { get; set; }
        public MaterialSign Relevance { get; set; }
        public MaterialSign Completeness { get; set; }
        public MaterialSign SourceReliability { get; set; }
        public MaterialSign ProcessedStatus { get; set; }
        public MaterialSign SessionPriority { get; set; }
        public JObject[] Transcriptions { get; set; }
        public MaterialDocument[] Children { get; set; }
        public int MlHandlersCount { get; set; }
        public int ProcessedMlHandlersCount { get; set; }
        public Guid[] NodeIds { get; set; }
        public int NodesCount { get; set; }
        public int ObjectsOfStudyCount { get; set; }
        public Assignee Assignee { get; set; }
        public Editor Editor { get; set; }
        public JObject MLResponses { get; set; }
        public string Title { get; set; }
        public ImageVector[] ImageVectors { get; set; }
        public IReadOnlyCollection<RelatedObject> RelatedObjectCollection { get; set; } = Array.Empty<RelatedObject>();
        public IReadOnlyCollection<RelatedObject> RelatedEventCollection { get; set; } = Array.Empty<RelatedObject>();
        public IReadOnlyCollection<RelatedObject> RelatedSignCollection { get; set; } = Array.Empty<RelatedObject>();
        public string RegistrationDate { get; set; }
        public SecurityAttributes SecurityAttributes { get; set; } = new SecurityAttributes();
    }

    public class MaterialSign
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public int OrderNumber { get; set; }
    }

    public class Assignee
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
    }

    public class Editor
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
    }

    public class MaterialLoadData
    {
        public string From { get; set; }
        public string LoadedBy { get; set; }
        public string Coordinates { get; set; }
        public string Code { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public IEnumerable<string> Objects { get; set; } = new List<string>();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public IEnumerable<string> States { get; set; } = new List<string>();
    }

    public class ImageVector
    {
        public decimal[] Vector { get; }

        public ImageVector(decimal[] imageVector)
        {
            Vector = imageVector;
        }
    }

    public class RelatedObject
    {
        public Guid Id { get; }
        public string Title { get; }
        public string NodeType { get; }
        public string RelationType { get; }
        public string RelationCreatingType { get; }
        public RelatedObject(Guid id, string title, string nodeType, string relationType, string relationCreatingType)
        {
            Id = id;
            Title = title;
            NodeType = nodeType;
            RelationType = relationType;
            RelationCreatingType = relationCreatingType;
        }
    }
}
