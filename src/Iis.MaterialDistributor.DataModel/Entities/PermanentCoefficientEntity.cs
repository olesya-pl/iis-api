namespace Iis.MaterialDistributor.DataModel.Entities
{
    public class PermanentCoefficientEntity
    {
        public const string RelatedToObjectOfStudy = "relatedToObjectOfStudy";
        public const int RelatedToObjectOfStudyValue = 100;
        public const string HasPhoneNumber = "hasPhoneNumber";
        public const int HasPhoneNumberValue = 50;
        public const string HasIridiumOptions = "hasIridiumOptions";
        public const int HasIridiumOptionsValue = 20;
        public const string HasTMSI = "hasTMSI";
        public const int HasTMSIValue = 10;
        public const string RelatedWithHighPriority = "relatedWithHighPriority";
        public const int RelatedWithHighPriorityValue = 5;
        public const string RelatedAndIgnoredPriority = "relatedAndIgnoredPriority";
        public const int RelatedAndIgnoredPriorityValue = -500;

        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}