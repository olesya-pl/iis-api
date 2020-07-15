using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using IIS.Core.Materials;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.GraphQL.Materials
{
    public class Mutation
    {
        // ReSharper disable once UnusedMember.Global
        public async Task<CreateMaterialResponse> CreateMaterial(
            [Service] IMaterialProvider materialProvider,
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [Service] IFeatureProcessorFactory featureProcessorFactory,
            [GraphQLNonNullType] MaterialInput input)
        {
            Iis.Domain.Materials.Material inputMaterial = mapper.Map<Iis.Domain.Materials.Material>(input);

            inputMaterial.Reliability = materialProvider.GetMaterialSign(input.ReliabilityText);

            inputMaterial.SourceReliability = materialProvider.GetMaterialSign(input.SourceReliabilityText);

            inputMaterial.LoadData = mapper.Map<Iis.Domain.Materials.MaterialLoadData>(input);

            inputMaterial.Metadata = await featureProcessorFactory.GetInstance(inputMaterial.Source, inputMaterial.Type).ProcessMetadata(inputMaterial.Metadata);

            await materialService.SaveAsync(inputMaterial);
            
            return new CreateMaterialResponse { Id = inputMaterial.Id };
        }

        public async Task<Material> UpdateMaterial(
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var material = await materialService.UpdateMaterialAsync(input);
            return mapper.Map<Material>(material);
        }
    }
}
