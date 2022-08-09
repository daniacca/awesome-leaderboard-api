using Common.DistribuitedCache.Manager.AbstractClasses;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace Common.DistribuitedCache.Manager.Commands
{
    public class RefreshCommand : AbsCommand<string>
    {
        public RefreshCommand(string key) : base(key)
        { }

        protected override async Task ExecuteMethodAsync(IDistributedCache receiver)
        {
            await receiver.RefreshAsync(CommandData);
        }

        protected override void ExecuteReceiverMethod(IDistributedCache receiver)
        {
            receiver.Refresh(CommandData);
        }
    }
}
