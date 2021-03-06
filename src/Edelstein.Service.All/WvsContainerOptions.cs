using System.Collections.Generic;
using Edelstein.Core.Services.Info;

namespace Edelstein.Service.All
{
    public class WvsContainerOptions : ServiceInfo
    {
        public ICollection<LoginServiceInfo> LoginServices { get; set; }
        public ICollection<GameServiceInfo> GameServices { get; set; }
        public ICollection<ShopServiceInfo> ShopServices { get; set; }
        public ICollection<TradeServiceInfo> TradeServices { get; set; }
    }
}