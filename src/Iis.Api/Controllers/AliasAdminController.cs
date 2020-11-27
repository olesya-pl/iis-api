using Iis.Interfaces.Enums;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("admin/aliases")]
    [ApiController]
    public class AliasAdminController : ControllerBase
    {
        public readonly IAliasService _aliasService;

        public AliasAdminController(IAliasService aliasService)
        {
            _aliasService = aliasService;
        }

        [HttpGet]
        public Task<List<AliasDto>> GetAsync([FromQuery] AliasType? type) 
        {
            
            return type.HasValue
                ? _aliasService.GetByTypeAsync(type.Value)
                : _aliasService.GetAllAsync();
        }

        [HttpPost]
        public Task<AliasDto> CreateAsync([FromBody] AliasDto dto)
        {
            return _aliasService.CreateAsync(dto);
        }

        [HttpDelete("{id}")]
        public Task<AliasDto> DeleteAsync([FromRoute] Guid id)
        {
            return _aliasService.RemoveAsync(id);
        }

        [HttpPut]
        public Task<AliasDto> UpdateAsync([FromBody] AliasDto dto)
        {
            return _aliasService.UpdateAsync(dto);
        }

        [HttpPost("list")]
        public Task<List<AliasDto>> CreateAliasesAsync([FromBody] List<AliasDto> aliases) 
        {
            return _aliasService.CreateAsync(aliases);
        }
    }
}
