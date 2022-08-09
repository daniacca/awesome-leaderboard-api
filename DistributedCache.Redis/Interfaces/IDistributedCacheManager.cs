using Microsoft.Extensions.Caching.Distributed;

namespace Common.DistribuitedCache.Interfaces
{
    public interface IDistributedCacheManager : ICommandInvoker<IDistributedCache>
    { }
}
