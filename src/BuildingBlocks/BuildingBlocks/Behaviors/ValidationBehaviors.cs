using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Behaviors;

public class ValidationBehaviors<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<TRequest> context = new(request);

        var validationResults = await Task.WhenAll(validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var failures = validationResults.Where(x => !x.IsValid).SelectMany(x => x.Errors).ToList();

        if (failures.Count > 0) throw new ValidationException(failures);

        return await next();
    }
}
