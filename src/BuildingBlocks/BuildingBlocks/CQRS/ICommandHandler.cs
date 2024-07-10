using MediatR;

namespace BuildingBlocks.CQRS;

public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
    where TCommand : ICommand<Unit>;

public interface ICommandHandler<in ICommand, TResponse> : IRequestHandler<ICommand, TResponse>
    where ICommand : ICommand<TResponse>
    where TResponse : notnull;
