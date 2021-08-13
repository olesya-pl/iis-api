using System;
using System.Collections.Generic;

namespace Iis.DataModel.Materials
{
    public class MaterialEntity : BaseEntity
    {
        #region Constants

        public static readonly Guid ProcessingStatusSignTypeId
            = new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34");

        public static readonly Guid ProcessingStatusProcessedSignId
            = new Guid("c85a76f4-3c04-46f7-aed9-f865243b058e");

        public static readonly Guid ProcessingStatusProcessingSignId
            = new Guid("AB3B68F3-42BB-4C43-8121-ED0DC2B0BAD1");

        public static readonly Guid ProcessingStatusPrimaryProcessingSignId
            = new Guid("5164D448-8709-4839-9907-F5205CA384D1");

        public static readonly Guid ProcessingStatusNotProcessedSignId
            = new Guid("0a641312-abb7-4b40-a766-0781308eb077");

        public static readonly Guid ImportanceFirstCategorySignId
            = new Guid("1107a504-c2a7-4f8b-a218-e5bbf5f281c4");

        public static readonly Guid ImportanceSecondCategorySignId
            = new Guid("1240c504-8ecd-4aca-9b75-24f0c6304426");

        public static readonly Guid ImportanceThirdCategorySignId
            = new Guid("1356a6b3-c63f-4985-8b74-372236fe744f");

        public static readonly Guid ReliabilityReliableSignId
            = new Guid("211f5765-0867-4d04-976a-70f3e34bf153");

        public static readonly Guid ReliabilityProbableSignId
            = new Guid("225f189b-9ad2-4687-9624-0d4c991a3d6b");

        public static readonly Guid ReliabilityDoubtfulSignId
            = new Guid("2326d6ef-5542-42a8-83eb-0c2b92d188f1");

        public static readonly Guid ReliabilityUnreliableSignId
            = new Guid("2475d991-b09e-4997-9d0a-2fc0bf07b1eb");

        public static readonly Guid ReliabilityUnknownReliabilitySignId
            = new Guid("25007914-d5cb-4def-8162-c12b4aa7038c");

        public static readonly Guid ReliabilityDesinformationSignId
            = new Guid("2616aa7d-c379-452b-8c1a-c815f9b989bc");

        public static readonly Guid RelevanceWarningSignId
            = new Guid("313d1f5b-7b3a-446f-ab92-e4046930a599");

        public static readonly Guid RelevanceVeryRelevantSignId
            = new Guid("320c2a19-ed1b-4250-bb02-eb4f7391165b");

        public static readonly Guid RelevanceRelevantSignId
            = new Guid("3317a961-1929-4957-9ef0-08b3007648a6");

        public static readonly Guid RelevanceAverageRelevanceSignId
            = new Guid("341892c9-3918-4a7f-bf61-d5b9050de7f4");

        public static readonly Guid RelevanceIrrelevantSignId
            = new Guid("354436fe-8c25-4352-8b89-3e94bf5828e2");

        public static readonly Guid CompletenessCompleteSignId
            = new Guid("4124de78-0877-40b4-834a-f892060ea3f5");

        public static readonly Guid CompletenessCompleteEnoughSignId
            = new Guid("422914a7-f761-4075-a91e-4d34d33aedff");

        public static readonly Guid CompletenessPartialSignId
            = new Guid("431a888f-406b-458a-9905-abc752710659");

        public static readonly Guid CompletenessNotEnoughDataSignId
            = new Guid("44ddf35a-eeee-4aa3-9f3c-9b73dc1d63ee");

        public static readonly Guid SourceReliabilityCompletelyReliableSignId
            = new Guid("513de8b4-5c99-414f-94f1-513a716fc01c");

        public static readonly Guid SourceReliabilityMostlyReliableSignId
            = new Guid("521ad86b-af5d-4731-b5e7-e3e69ef23fc7");

        public static readonly Guid SourceReliabilityRelativelyReliableSignId
            = new Guid("5342ead6-d478-4abc-b8d1-fd5d6a741706");

        public static readonly Guid SourceReliabilityRarelyReliableSignId
            = new Guid("5406768c-581d-4b95-a549-b2cd1d09cfd8");

        public static readonly Guid SourceReliabilityCannotEstimateSignId
            = new Guid("56365559-24fb-42f2-8305-bbef01fd6e3e");

        public static readonly Guid SourceReliabilityUnreliableSignId
            = new Guid("55b0a038-2347-4fb0-82e1-6081933ac9e1");

        public static readonly Guid SessionPriorityImportantSignId
            = new Guid("6051013d-846b-4409-9da0-9414b103b396");

        public static readonly Guid SessionPriorityImmediateReportSignId
            = new Guid("60536b3c-e78d-40cf-9199-37f112184f69");

        public static readonly Guid SessionPrioritySkipSignId
            = new Guid("60560b14-195c-4605-816e-983118ab9ed9");

        public static readonly Guid SessionPriorityTranslateSignId
            = new Guid("6071f9f3-1988-4b14-9f6a-c0514fc795d0");

        #endregion

        public Guid? ParentId { get; set; }
        public virtual MaterialEntity Parent { get; set; }
        public Guid? FileId { get; set; }
        public virtual FileEntity File { get; set; }
        public string Metadata { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid? ImportanceSignId { get; set; }
        public Guid? ReliabilitySignId { get; set; }
        public Guid? RelevanceSignId { get; set; }
        public Guid? CompletenessSignId { get; set; }
        public Guid? SourceReliabilitySignId { get; set; }
        public Guid? SessionPriorityId { get; set; }
        public MaterialSignEntity Importance { get; set; }
        public MaterialSignEntity Reliability { get; set; }
        public MaterialSignEntity Relevance { get; set; }
        public MaterialSignEntity Completeness { get; set; }

        public MaterialSignEntity SourceReliability { get; set; }
        public MaterialSignEntity SessionPriority { get; set; }
        public string Title { get; set; }
        public string LoadData { get; set; }
        public Guid? ProcessedStatusSignId { get; set; }
        public MaterialSignEntity ProcessedStatus { get; set; }
        public virtual ICollection<MaterialEntity> Children { get; set; }
        public virtual ICollection<MaterialInfoEntity> MaterialInfos { get; set; }
        public Guid? AssigneeId { get; set; }
        public virtual UserEntity Assignee { get; set; }
        public int MlHandlersCount { get; set; }
        public int AccessLevel { get; set; }

        public Guid? EditorId { get; set; }
        public virtual UserEntity Editor { get; set; }

        public bool CanBeEdited(Guid userId)
        {
            if (ProcessedStatusSignId == ProcessingStatusProcessingSignId)
                return EditorId == userId;

            return true;
        }

        public bool CanBeAccessedBy(int accessLevel) => accessLevel >= AccessLevel;
    }
}