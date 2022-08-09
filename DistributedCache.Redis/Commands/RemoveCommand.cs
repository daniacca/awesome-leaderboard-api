using Common.DistribuitedCache.Manager.AbstractClasses;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace Common.DistribuitedCache.Manager.Commands
{
    public class RemoveCommand : AbsCommand<string>
    {
        public RemoveCommand(string key) : base(key)
        { }

        protected override async Task ExecuteMethodAsync(IDistributedCache receiver)
        {
            await receiver.RemoveAsync(CommandData);
        }

        protected override void ExecuteReceiverMethod(IDistributedCache receiver)
        {
            receiver.Remove(CommandData);
        }
    }
}
