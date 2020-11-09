﻿using Iis.Interfaces.Enums;
using Iis.Services.Contracts.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAliasService
    {
        Task<AliasDto> CreateAsync(AliasDto aliasDto);
        Task<List<AliasDto>> GetAllAsync();
        Task<List<AliasDto>> GetByTypeAsync(AliasType type);
    }
}