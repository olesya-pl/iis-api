namespace Iis.Interfaces.Elastic
{
    public class Property
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public Property()
        {
            
        }
        public Property(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}