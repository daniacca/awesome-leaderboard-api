using Common.DistribuitedCache.Manager.AbstractClasses;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.DistribuitedCache.Manager.Commands
{
    public class SetCommandPayload<TData> where TData : class
    {
        public string Key { get; set; }
        public TData Data { get; set; }
        public DistributedCacheEntryOptions Options { get; set; }
    }

    public class SetCommand<TData> : AbsCommand<SetCommandPayload<TData>> where TData : class
    {
        public SetCommand(SetCommandPayload<TData> payload) : base(payload)
        { }

        public SetCommand(string key, TData data, DistributedCacheEntryOptions options = null) : base(new SetCommandPayload<TData>{ Data = data, Key = key, Options = options })
        { }

        protected DistributedCacheEntryOptions GetOptions()
        {
            var Options = CommandData.Options ?? new DistributedCacheEntryOptions();
            if (CommandData.Options is null)
                Options.SetAbsoluteExpiration(DateTimeOffset.Now.AddHours(1));
            return Options;
        }

        protected override void ExecuteReceiverMethod(IDistributedCache receiver)
        {
            string jsonData = JsonSerializer.Serialize(CommandData.Data);
            receiver.SetString(CommandData.Key, jsonData, GetOptions());
        }

        protected override async Task ExecuteMethodAsync(IDistributedCache receiver)
        {
            string jsonData = JsonSerializer.Serialize(CommandData.Data);
            await receiver.SetStringAsync(CommandData.Key, jsonData, GetOptions());
        }
    }

    public class SetCommand : SetCommand<string>
    {
        public SetCommand(SetCommandPayload<string> payload) : base(payload)
        { }

        public SetCommand(string key, string data, DistributedCacheEntryOptions options = null) : base(key, data, options)
        { }

        protected override void ExecuteReceiverMethod(IDistributedCache receiver)
        {
            receiver.SetString(CommandData.Key, CommandData.Data, GetOptions());
        }

        protected override async Task ExecuteMethodAsync(IDistributedCache receiver)
        {
            await receiver.SetStringAsync(CommandData.Key, CommandData.Data, GetOptions());
        }
    }
}
