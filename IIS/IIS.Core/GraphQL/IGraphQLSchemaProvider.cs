﻿using System;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;

namespace IIS.Core.GraphQL
{
    public interface IGraphQLSchemaProvider
    {
        ISchema GetSchema();
    }
}
