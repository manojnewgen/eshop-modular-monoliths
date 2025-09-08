using MediatR;

namespace Shared.Contracts.CQRS
{
    // Fix: Add generic constraint to ensure TCommand implements ICommand<Unit>
    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
        where TCommand : ICommand<Unit>
    {
    }
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : notnull
    {
    }
}
