using System;
using System.Collections.Generic;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.Ontology.Seeding
{
    public static class BuilderExtensions
    {
        public static ITypeBuilder HasRequired(this ITypeBuilder builder, ITypeBuilder propertyBuilder) =>
            builder.HasRequired(((OntologyBuilder) propertyBuilder).Name);

        public static ITypeBuilder HasOptional(this ITypeBuilder builder, ITypeBuilder propertyBuilder) =>
            builder.HasOptional(((OntologyBuilder) propertyBuilder).Name);

        public static ITypeBuilder HasMultiple(this ITypeBuilder builder, ITypeBuilder propertyBuilder) =>
            builder.HasMultiple(((OntologyBuilder) propertyBuilder).Name);



        public static ITypeBuilder HasRequired(this ITypeBuilder builder, ITypeBuilder propertyBuilder, string name, string title) =>
            builder.HasRequired(r => r.Target(propertyBuilder).WithName(name).WithTitle(title));

        public static ITypeBuilder HasOptional(this ITypeBuilder builder, ITypeBuilder propertyBuilder, string name, string title) =>
            builder.HasOptional(r => r.Target(propertyBuilder).WithName(name).WithTitle(title));

        public static ITypeBuilder HasMultiple(this ITypeBuilder builder, ITypeBuilder propertyBuilder, string name, string title) =>
            builder.HasMultiple(r => r.Target(propertyBuilder).WithName(name).WithTitle(title));



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

        public static ITypeBuilder CreateEnum(this OntologyBuildContext ctx, string enumName) =>
            ctx.CreateBuilder().IsEntity().Is("Enum").WithName(enumName);


        public static ITypeBuilder RejectEmbeddedOperations(this ITypeBuilder builder) =>
            builder.WithMeta(new EntityMeta {AcceptsEmbeddedOperations = new EntityOperation[] { }});

        public static ITypeBuilder AcceptEmbeddedOperations(this ITypeBuilder builder) =>
            builder.WithMeta(new EntityMeta {AcceptsEmbeddedOperations = new []
                {EntityOperation.Create, EntityOperation.Update, EntityOperation.Delete}});


        public static IRelationBuilder Target(this IRelationBuilder builder, ITypeBuilder propertyBuilder) =>
            builder.Target(((OntologyBuilder) propertyBuilder).Name);

        public static IRelationBuilder WithEmptyMeta(this IRelationBuilder builder) =>
            builder.WithMeta<EntityRelationMeta>(m => { });

        public static IRelationBuilder WithFormula(this IRelationBuilder builder, string formula) =>
            builder.WithMeta<AttributeRelationMeta>(m => { m.Formula = formula; });

        public static IRelationBuilder WithFormFieldType(this IRelationBuilder builder, string formFieldType) =>
            builder.WithMeta<EntityRelationMeta>(m => m.FormField = new FormField {Type = formFieldType});


        public static IRelationBuilder HasInversed(this IRelationBuilder builder, Action<InversedRelationBuilder> inversedDescriptor)
        {
            var inversedBuilder = new InversedRelationBuilder();
            inversedDescriptor(inversedBuilder);
            var inversedMeta = new InversedRelationMeta();
            inversedBuilder.Build()(inversedMeta);
            return builder.WithMeta<EntityRelationMeta>(m => m.Inversed = inversedMeta);
        }

        public static IRelationBuilder HasEntityMeta(this IRelationBuilder builder, Action<EntityRelationMeta> descriptor) =>
            builder.WithMeta(descriptor);

        public static IRelationBuilder HasAttributeMeta(this IRelationBuilder builder, Action<AttributeRelationMeta> descriptor) =>
            builder.WithMeta(descriptor);

        public static IRelationBuilder HasOperations(this IRelationBuilder builder, EntityOperation[] operations) =>
            builder.WithMeta<EntityRelationMeta>(d => d.AcceptsEntityOperations = operations);

        public static IRelationBuilder HasCUDOperations(this IRelationBuilder builder) =>
            builder.HasOperations(new [] {EntityOperation.Create, EntityOperation.Update, EntityOperation.Delete});

        public static IRelationBuilder HasNoneOperations(this IRelationBuilder builder) =>
            builder.HasOperations(new EntityOperation[] { });
    }

    public class InversedRelationBuilder
    {
        private List<Action<InversedRelationMeta>> _actions = new List<Action<InversedRelationMeta>>();

        public InversedRelationBuilder WithName(string name) => With(m => m.Code = name.ToLowerCamelcase());

        public InversedRelationBuilder WithTitle(string title) => With(m => m.Title = title);

        public InversedRelationBuilder IsMultiple() => With(m => m.Multiple = true);

        public InversedRelationBuilder With(Action<InversedRelationMeta> descriptor)
        {
            _actions.Add(descriptor);
            return this;
        }

        public Action<InversedRelationMeta> Build() =>
            m =>
            {
                foreach (var a in _actions)
                    a(m);
            };
    }
}
