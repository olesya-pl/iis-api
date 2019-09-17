using System;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.Ontology.Seeding
{
    public static class BuilderExtensions
    {
        public static ITypeBuilder HasRequired(this ITypeBuilder builder, ITypeBuilder propertyBuilder,
            string relationName = null, RelationMetaBase meta = null, string title = null) =>
            builder.HasRequired(((OntologyBuilder) propertyBuilder).Name, relationName, meta?.Serialize(), title);

        public static ITypeBuilder HasOptional(this ITypeBuilder builder, ITypeBuilder propertyBuilder,
            string relationName = null, RelationMetaBase meta = null, string title = null) =>
            builder.HasOptional(((OntologyBuilder) propertyBuilder).Name, relationName, meta?.Serialize(), title);

        public static ITypeBuilder HasMultiple(this ITypeBuilder builder, ITypeBuilder propertyBuilder,
            string relationName = null, RelationMetaBase meta = null, string title = null) =>
            builder.HasMultiple(((OntologyBuilder) propertyBuilder).Name, relationName, meta?.Serialize(), title);


        public static ITypeBuilder HasRequired(this ITypeBuilder builder, string propertyName,
            string relationName = null, RelationMetaBase meta = null, string title = null) =>
            builder.HasRequired(propertyName, relationName, meta?.Serialize(), title);

        public static ITypeBuilder HasOptional(this ITypeBuilder builder, string propertyName,
            string relationName = null, RelationMetaBase meta = null, string title = null) =>
            builder.HasOptional(propertyName, relationName, meta?.Serialize(), title);

        public static ITypeBuilder HasMultiple(this ITypeBuilder builder, string propertyName,
            string relationName = null, RelationMetaBase meta = null, string title = null) =>
            builder.HasMultiple(propertyName, relationName, meta?.Serialize(), title);


        public static ITypeBuilder Is(this ITypeBuilder builder, ITypeBuilder propertyBuilder) =>
            builder.Is(((OntologyBuilder) propertyBuilder).Name);


        public static ITypeBuilder HasRequired(this ITypeBuilder builder, OntologyBuildContext context,
            Func<ITypeBuilder, ITypeBuilder> descriptor) =>
            builder.HasRequired(descriptor(context.CreateBuilder()));

        public static ITypeBuilder HasOptional(this ITypeBuilder builder, OntologyBuildContext context,
            Func<ITypeBuilder, ITypeBuilder> descriptor) =>
            builder.HasOptional(descriptor(context.CreateBuilder()));

        public static ITypeBuilder HasMultiple(this ITypeBuilder builder, OntologyBuildContext context,
            Func<ITypeBuilder, ITypeBuilder> descriptor) =>
            builder.HasMultiple(descriptor(context.CreateBuilder()));
    }
}
