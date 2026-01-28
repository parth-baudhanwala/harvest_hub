using BuildingBlocks.MessageBroker.Events;
using Catalog.Api.Storage;
using MassTransit;

namespace Catalog.Api.Products.CreateProduct;

public record CreateProductCommand(string Name,
                                   List<string> Categories,
                                   string Description,
                                   string ImageFileName,
                                   byte[] ImageBytes,
                                   string? ImageContentType,
                                   decimal Price) : ICommand<CreateProductResult>;

public record CreateProductResult(Guid Id);

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.")
                            .Length(3, 150).WithMessage("Name must be between 3 to 150 characters.");
        RuleFor(x => x.Categories).NotEmpty().WithMessage("Categories are required.");
        RuleFor(x => x.ImageFileName).NotEmpty().WithMessage("Image file name is required.");
        RuleFor(x => x.ImageBytes).NotNull().Must(bytes => bytes.Length > 0)
                                  .WithMessage("Image bytes are required.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}

internal class CreateProductCommandHandler(IDocumentSession session, IProductImageStorage imageStorage, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        Guid productId = Guid.NewGuid();
        ImageUploadResult uploadResult = await imageStorage.UploadAsync(
            productId,
            command.ImageFileName,
            command.ImageBytes,
            command.ImageContentType,
            cancellationToken);

        Product product = new()
        {
            Id = productId,
            Name = command.Name,
            Categories = command.Categories,
            Description = command.Description,
            ImageFile = uploadResult.Url,
            ImageObjectKey = uploadResult.ObjectKey,
            ImageFileName = uploadResult.FileName,
            ImageContentType = uploadResult.ContentType,
            ImageSizeBytes = uploadResult.SizeBytes,
            Price = command.Price
        };

        session.Store(product);
        await session.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new ProductUpsertedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price
        }, cancellationToken);

        return new CreateProductResult(product.Id);
    }
}
