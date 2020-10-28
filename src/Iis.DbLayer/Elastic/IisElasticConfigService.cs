using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Elastic;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using IIS.Repository;
using IIS.Repository.Factories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.DbLayer.Elastic
{
    public class IisElasticConfigService<TUnitOfWork> : BaseService<TUnitOfWork>, IIisElasticConfigService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IMapper _mapper;

        public IisElasticConfigService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory, IMapper mapper) : base(unitOfWorkFactory)
        {
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<IElasticFieldEntity>> SaveElasticFieldsAsync(string typeName, IReadOnlyList<IIisElasticField> fields)
        {
            var existingFields = await RunWithoutCommitAsync(
                async uow => await uow.ElasticFieldsRepository.GetElasticFieldsByTypename(typeName));
            var entitiesToAdd = new List<ElasticFieldEntity>();
            var entititesToUpdate = new List<ElasticFieldEntity>();
            foreach (var field in fields)
            {
                if (existingFields.ContainsKey(field.Name))
                {
                    var existingField = existingFields[field.Name];
                    _mapper.Map(field, existingField);
                    entititesToUpdate.Add(existingField);
                    //await RunAsync(
                    //    async uow => await uow.ElasticFieldsRepository.UpdateField(existingField));
                }
                else
                {
                    var fieldEntity = _mapper.Map<ElasticFieldEntity>(field);
                    fieldEntity.Id = Guid.NewGuid();
                    fieldEntity.TypeName = typeName;
                    fieldEntity.ObjectType = ElasticObjectType.Ontology;
                    entitiesToAdd.Add(fieldEntity);                    
                }
            }
            await RunAsync(
                        async uow => await uow.ElasticFieldsRepository.AddField(entitiesToAdd, entititesToUpdate));

            return await RunWithoutCommitAsync(
                async uow => await uow.ElasticFieldsRepository.GetReadonlyElasticFieldsByTypename(typeName));;
        }
    }
}
