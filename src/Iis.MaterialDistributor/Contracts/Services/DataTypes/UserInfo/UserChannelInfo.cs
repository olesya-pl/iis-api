namespace Iis.MaterialDistributor.Contracts.Services
{
    public class UserChannelInfo
    {
        private static readonly UserChannelInfo _default = new UserChannelInfo { Coefficient = 1 };
        public static UserChannelInfo Default => _default;
        public string Channel { get; set; }
        public short Coefficient { get; set; }
    }
}