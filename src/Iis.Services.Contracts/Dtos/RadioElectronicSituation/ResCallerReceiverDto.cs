using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Dtos.RadioElectronicSituation
{
    public class ResCallerReceiverDto
    {
        public Guid CallerId { get; set; }
        public Guid ReceiverId { get; set; }
        public int Count { get; set; }
    }
}
