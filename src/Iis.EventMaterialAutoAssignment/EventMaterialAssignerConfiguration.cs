namespace Iis.EventMaterialAutoAssignment
{
    public class EventMaterialAssignerConfiguration
    {
        public string IncomingQueueName { get; set; } = "materials.termcheck.foundmaterials";
        public string IncomingRoutingKey { get; set; } = "materials";
        public string IncomingExchangeName { get; set; } = "terms-checker";
    }
}
