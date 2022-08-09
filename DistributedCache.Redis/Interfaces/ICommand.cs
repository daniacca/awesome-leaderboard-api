using System.Threading.Tasks;

namespace Common.DistribuitedCache.Interfaces
{
    public interface ICommand<TReceiver>
    {
        void Execute(TReceiver receiver);
        Task ExecuteAsync(TReceiver receiver);
    }

    public interface ICommand<TOut, TReceiver> : ICommand<TReceiver>
    {
        TOut Result { get; }
    }
}
