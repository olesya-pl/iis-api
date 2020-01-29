using System;
using MediatR;

namespace Iis.Application.Ontology
{
    public sealed class SaveOntologyCommand : IRequest
    {
        public string Profile { get; }

        public SaveOntologyCommand(string profile)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }
    }
}
