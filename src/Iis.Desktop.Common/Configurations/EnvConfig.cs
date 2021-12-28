namespace Iis.Desktop.Common.Configurations
{
    public class EnvConfig
    {
        public const string SectionName = "environmentProperties";
        public int SortOrder { get; set; }
        public string ConnectionString { get; set; }
        public string ApiUri { get; set; }
        public string AppUri { get; set; }
    }
}
