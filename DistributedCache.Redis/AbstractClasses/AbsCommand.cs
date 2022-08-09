using Common.DistribuitedCache.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Common.DistribuitedCache.Manager.AbstractClasses
{
    public abstract class AbsCommand<TIn> : ICommand<IDistributedCache>
    {
        protected TIn CommandData { get; }

        protected AbsCommand(TIn data)
        {
            CommandData = data;
        }

        protected abstract void ExecuteReceiverMethod(IDistributedCache receiver);

        protected abstract Task ExecuteMethodAsync(IDistributedCache receiver);

        public virtual void Execute(IDistributedCache receiver)
        {
            try
            {
                ExecuteReceiverMethod(receiver);
            }
            catch (Exception)
            {
                // TODO LOG Exception
            }
        }

        public async Task ExecuteAsync(IDistributedCache receiver)
        {
            try
            {
                await ExecuteMethodAsync(receiver);
            }
            catch (Exception)
            {
                // TODO LOG Exception
            }
        }
    }

    public abstract class AbsCommand<TIn, TOut> : AbsCommand<TIn>, ICommand<TOut, IDistributedCache>
    {
        public TOut Result { get; protected set; }

        protected AbsCommand(TIn data) : base(data)
        { }
    }
}
