using BuildingBlocks.MessageBroker.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderStream.Application.Data;
using OrderStream.Domain.Models;
using OrderStream.Domain.ValueObjects;

namespace OrderStream.Application.Orders.EventHandlers.Integration;

public class ProductUpsertedEventHandler(IApplicationDbContext dbContext, ILogger<ProductUpsertedEventHandler> logger)
    : IConsumer<ProductUpsertedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpsertedEvent> context)
    {
        logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);

        if (context.Message.ProductId == Guid.Empty)
        {
            logger.LogWarning("ProductUpsertedEvent missing ProductId.");
            return;
        }

        var productId = ProductId.Of(context.Message.ProductId);
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId, context.CancellationToken);

        if (product is null)
        {
            var newProduct = Product.Create(productId, context.Message.Name, context.Message.Price);
            dbContext.Products.Add(newProduct);
        }
        else
        {
            product.UpdateDetails(context.Message.Name, context.Message.Price);
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
