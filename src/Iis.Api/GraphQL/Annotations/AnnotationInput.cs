using HotChocolate;

namespace IIS.Core.GraphQL.Annotations
{
    public class AnnotationInput : BaseAnnotation
    {
        [GraphQLNonNullType]
        public string Content { get; set; }
    }
}