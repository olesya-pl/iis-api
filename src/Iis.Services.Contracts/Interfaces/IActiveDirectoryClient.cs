using System;
using System.Collections.Generic;
using Iis.Services.Contracts.Dtos;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IActiveDirectoryClient
    {
        List<ActiveDirectoryGroupDto> GetAllGroups();

        List<ActiveDirectoryGroupDto> GetGroupsByIds(params Guid[] ids);
    }
}