﻿using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Dtos.RadioElectronicSituation
{
    public class ResSourceItemDto
    {
        public LocationHistoryDto LocationHistory { get; private set; }
        public INode Sign { get; private set; }
        public INode ObjectNode { get; private set; }
        public ResMaterialDto Material { get; private set; }
        public ResSourceItemDto(
            LocationHistoryDto locationHistory,
            INode sign,
            INode objectNode,
            ResMaterialDto material)
        {
            LocationHistory = locationHistory;
            Sign = sign;
            ObjectNode = objectNode;
            Material = material;
        }
    }
}
