﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class UserDistributionDto
    {
        public Guid Id { get; }
        public IReadOnlyList<string> RoleNames { get; }
        public int FreeSlots { get; set; }
        public UserDistributionDto() { }
        public UserDistributionDto(Guid id, int freeSlots, IEnumerable<string> roleNames)
        {
            Id = id;
            FreeSlots = freeSlots;
            RoleNames = roleNames?.ToList() ?? new List<string>();
        }

        public int GetPriority(string roleName)
        {
            if (RoleNames.Count == 0) return 0;
            if (!RoleNames.Contains(roleName)) return -1;
            if (RoleNames.Count == 1) return 2;
            return 1;
        }
    }
}
