namespace Iis.EventMaterialAutoAssignment
{
    public class MaterialMessageHandlerConfiguration
    {
        public string IncomingQueueName { get; set; } = "materials.elastic.saved";
        public string OutgoingQueueName { get; set; } = "materials.processing.termcheck";
        public string OutgoingExchangeName { get; set; } = "materials";
        public string OutgoingRoutingKey { get; set; } = "terms-checker";
        public string IncomingRoutingKey { get; set; } = "materials";
        public string IncomingExchangeName { get; set; } = "materials.elastic.saved";
    }
}