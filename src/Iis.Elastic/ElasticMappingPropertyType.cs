namespace Iis.Elastic
{
    public enum ElasticMappingPropertyType : byte
    {
        Text,
        Integer,
        Date,
        Nested,
        Alias,
        Keyword,
        DenseVector,
        DateRange,
        IntegerRange,
        FloatRange
    }
}
