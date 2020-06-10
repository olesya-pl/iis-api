using System.Linq;
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
        public async Task<Material> CreateMaterial(
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

            var featureProcessor = featureProcessorFactory.GetInstance(inputMaterial.Source);
            
            var res = featureProcessor.ProcessMetadata(inputMaterial.Metadata);
            
            throw new System.Exception("FUCK");
            
            await materialService.SaveAsync(inputMaterial);

            Iis.Domain.Materials.Material material = await materialProvider.GetMaterialAsync(inputMaterial.Id);

            return mapper.Map<Material>(material);
        }

        public async Task<Material> UpdateMaterial(
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MaterialUpdateInput input)
        {
            var material = await materialService.UpdateMaterial(input);
            return mapper.Map<Material>(material);
        }

        public async Task<Material> AssignMaterialOperator(
            [Service] IMaterialService materialService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] AssignMaterialOperatorInput input)
        {
            var material = await materialService.AssignMaterialOperatorAsync(input.MaterialId, input.AssigneeId);
            return mapper.Map<Material>(material);
        }
    }
}
