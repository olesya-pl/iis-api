using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.ML
{
    public class MlProcessingService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;

        public MlProcessingService(OntologyContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<MlProcessingResult> GetMlHandlingResults(Guid materialId)
        {
            var data = _context.MLResponses
                .AsNoTracking()
                .Where(p => p.MaterialId == materialId)
                .ToList();
            return data.Select(p => _mapper.Map<MlProcessingResult>(p)).ToList();
        }
    }
}
