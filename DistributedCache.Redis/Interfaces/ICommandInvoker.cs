using System.Threading.Tasks;

namespace Common.DistribuitedCache.Interfaces
{
    public interface ICommandInvoker<TReceiver>
    {
        void Execute<TCommand>(TCommand command) where TCommand : ICommand<TReceiver>;

        Task ExecuteAsync<TCommand>(TCommand command) where TCommand : ICommand<TReceiver>;
    }
}
