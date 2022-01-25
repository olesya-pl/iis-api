﻿using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("securityLevel")]
    [ApiController]
    public class SecurityLevelController : ControllerBase
    {
        private readonly ISecurityLevelService _securityLevelService;
        private readonly IUserService _userService;

        public SecurityLevelController(
            ISecurityLevelService securityLevelService,
            IUserService userService)
        {
            _securityLevelService = securityLevelService;
            _userService = userService;
        }

        [HttpGet("getSecurityLevels")]
        public IReadOnlyList<SecurityLevelPlain> GetSecurityLevels() => _securityLevelService.GetSecurityLevelsPlain();

        [HttpGet("getUserSecurityDtos")]
        public Task<IReadOnlyList<UserSecurityDto>> GetUserSecurityDtosAsync()
            => _userService.GetUserSecurityDtosAsync();

        [HttpPost("saveUserSecurityDto")]
        public Task SaveUserSecurityDtoAsync(UserSecurityDto userSecurityDto)
            => _userService.SaveUserSecurityAsync(userSecurityDto);

        [HttpGet("getObjectSecurityDtos/{id}")]
        public Task<ObjectSecurityDto> GetObjectSecurityDtosAsync(Guid id) =>
            _securityLevelService.GetObjectSecurityDtosAsync(id);

        [HttpPost("saveObjectSecurityDto")]
        public Task SaveObjectSecurityDtoAsync(ObjectSecurityDto objectSecurityDto) =>
            _securityLevelService.SaveObjectSecurityDtoAsync(objectSecurityDto);

        [HttpPost("saveSecurityLevel")]
        public void SaveSecurityLevel(SecurityLevelPlain levelPlain) =>
            _securityLevelService.SaveSecurityLevel(levelPlain);

        [HttpPost("removeSecurityLevel")]
        public void RemoveSecurityLevel(SecurityLevelPlain levelPlain) =>
            _securityLevelService.RemoveSecurityLevel(levelPlain.Id);
    }
}