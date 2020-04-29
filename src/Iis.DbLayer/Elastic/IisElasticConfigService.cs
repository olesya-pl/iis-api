using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.Elastic;
using Iis.Interfaces.Elastic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iis.DbLayer.Elastic
{
    public class IisElasticConfigService : IIisElasticConfigService
    {
        OntologyContext _context;
        IMapper _mapper;

        public IisElasticConfigService(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<IElasticFieldEntity>> SaveElasticFieldsAsync(string typeName, IReadOnlyList<IIisElasticField> fields)
        {
            var existingFields = _context.ElasticFields.Where(ef => ef.TypeName == typeName).ToDictionary(ef => ef.Name, ef => ef);
            foreach (var field in fields)
            {
                if (existingFields.ContainsKey(field.Name))
                {
                    var existingField = existingFields[field.Name];
                    _mapper.Map(field, existingField);
                }
                else
                {
                    var fieldEntity = _mapper.Map<ElasticFieldEntity>(field);
                    fieldEntity.Id = Guid.NewGuid();
                    fieldEntity.TypeName = typeName;
                    fieldEntity.ObjectType = ElasticObjectType.Ontology;
                    await _context.ElasticFields.AddAsync(fieldEntity);
                }
            }
            _context.SaveChanges();
            return _context.ElasticFields.Where(ef => ef.TypeName == typeName).AsNoTracking().ToList();
        }
    }
}
