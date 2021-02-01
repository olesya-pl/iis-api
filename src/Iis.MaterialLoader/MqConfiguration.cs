namespace Iis.MaterialLoader
{
    /// <summary>
    /// Type for message queue configuration
    /// </summary>
    public class MqConfiguration
    {
        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
    }
}