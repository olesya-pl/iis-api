using System;
using System.Threading.Tasks;
using AutoMapper;

using IIS.Repository;
using IIS.Repository.Factories;
using Iis.DbLayer.Repositories;
using Iis.DataModel.Annotations;
using Iis.Services.Contracts.Annotations;

namespace Iis.Services
{
    public class AnnotationsService : BaseService<IIISUnitOfWork>, IAnnotationsService
    {
        private readonly IMapper _mapper;

        public AnnotationsService(
            IMapper mapper,
            IUnitOfWorkFactory<IIISUnitOfWork> uowFactory) : base(uowFactory)
        {
            _mapper = mapper;
        }
        public async Task<Annotation> GetAnnotationAsync(Guid annotationId)
        {
            var entity = await RunWithoutCommitAsync(uow => uow.AnnotationsRepository.GetByIdAsync(annotationId));

            return _mapper.Map<Annotation>(entity);
        }

        public async Task<Annotation> SetAnnotationAsync(Annotation annotation)
        {
            Func<IIISUnitOfWork, Task> action = null;;

            var entity = _mapper.Map<AnnotationEntity>(annotation);
            
            var existingEntity = await RunWithoutCommitAsync(uow => uow.AnnotationsRepository.GetByIdAsync(entity.Id));

            if(existingEntity is null)
            {
                action = uow => uow.AnnotationsRepository.AddAsync(entity);
            }
            else
            {
                existingEntity.Content = entity.Content;
                
                action = uow => uow.AnnotationsRepository.UpdateAsync(existingEntity);
            }

            await RunAsync(action);

            return _mapper.Map<Annotation>(entity);
        }
    }
}