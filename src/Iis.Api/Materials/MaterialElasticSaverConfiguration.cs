namespace Iis.Api.Materials
{
    public class MaterialElasticSaverConfiguration
    {
        public string QueueName { get; set; } = "materials.save.elastic";
        public string OutgoingQueueName { get; set; } = "materials.elastic.saved";
        public string OutgoingExchangeName { get; set; } = "materials";
        public string OutgoingRoutingKey { get; set; } = "materials.elastic.saved";
    }
}
