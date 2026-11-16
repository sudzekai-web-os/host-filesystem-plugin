using Application.Dispatchering.Commands;

namespace Application.Dispatchering.Dispatchers
{
    public interface IDispatcher
    {
        TResult Execute<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>;
    }
}
