using Common.DistribuitedCache.Manager.AbstractClasses;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.DistribuitedCache.Manager.Commands
{
    public class GetCommand<TOut> : AbsCommand<string, TOut> where TOut : class
    {
        public GetCommand(string key) : base(key)
        { }

        protected override async Task ExecuteMethodAsync(IDistributedCache receiver)
        {
            var stringResult = await receiver.GetStringAsync(CommandData);
            Result = JsonSerializer.Deserialize<TOut>(stringResult, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected override void ExecuteReceiverMethod(IDistributedCache receiver)
        {
            var stringResult = receiver.GetString(CommandData);
            Result = JsonSerializer.Deserialize<TOut>(stringResult, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }

    public class GetCommand : GetCommand<string>
    {
        public GetCommand(string key) : base(key)
        { }

        protected override void ExecuteReceiverMethod(IDistributedCache receiver)
        {
            Result = receiver.GetString(CommandData);
        }

        protected override async Task ExecuteMethodAsync(IDistributedCache receiver)
        {
            Result = await receiver.GetStringAsync(CommandData);
        }
    }

}
