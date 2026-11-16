using Application.Dispatchering.Commands;
using Application.Dispatchering.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Dispatchering.Dispatchers
{
    internal sealed class Dispatcher(IServiceProvider services) : IDispatcher
    {
        public TResult Execute<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>
            => services.GetRequiredService<ICommandHandler<TCommand, TResult>>().Handle(command);
    }
}
