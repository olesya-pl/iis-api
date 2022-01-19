using MediatR;

namespace Iis.MaterialDistributor.Contracts.Events
{
    public class ReadAllMaterialsEvent : INotification
    {
        public int HourOffset { get; set; }
    }
}