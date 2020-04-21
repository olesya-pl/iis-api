using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.Domain.MachineLearning;
namespace IIS.Core.ML
{
    public class MlProcessingService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;

        public MlProcessingService(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<MlResponse> AddMlHandlerResponseAsync(MlResponse response)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                var entity = _mapper.Map<MlResponse, MLResponseEntity>(response);
                _context.MLResponses.Add(entity);
                _context.SaveChanges();
                return  _mapper.Map<MlResponse>(entity);
            }
            finally 
            { 
                _context.Semaphore.Release(); 
            }
        }
        public IEnumerable<MlProcessingResult> GetMlHandlingResults(Guid materialId)
        {
            return _context.MLResponses
                .AsNoTracking()
                .Where(p => p.MaterialId == materialId)
                .Select(p => _mapper.Map<MlProcessingResult>(p))
                .ToList();
        }
    }
}
