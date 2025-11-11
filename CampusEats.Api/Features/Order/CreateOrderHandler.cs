using CampusEats.Api.Domain.Entities;
using CampusEats.Api.Features.Order.DTOs;
using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Order;

public record CreateOrderCommand(
    Guid UserId, 
    List<OrderItemRequest> Items
) : IRequest<Result<Guid>>; 

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly CampusEatsDbContext _context;
    private readonly IMediator _mediator;

    public CreateOrderHandler(CampusEatsDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, ct);
        if (!userExists)
            return Result.Failure<Guid>("User not found.");

        if (request.Items == null || !request.Items.Any())
            return Result.Failure<Guid>("Order cannot be empty.");
        
        var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
        
        var menuItemsFromDb = await _context.MenuItems
            .Where(m => menuItemIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Price, ct);
        
        if (menuItemIds.Any(id => !menuItemsFromDb.ContainsKey(id)))
            return Result.Failure<Guid>("One or more menu items are invalid.");
        
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            decimal totalPrice = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemRequest in request.Items)
            {
                var price = menuItemsFromDb[itemRequest.MenuItemId];
                totalPrice += price * itemRequest.Quantity;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = itemRequest.MenuItemId,
                    Quantity = itemRequest.Quantity,
                    Price = price 
                });
            }
            
            var newOrder = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Status = OrderStatus.Pending, 
                OrderDate = DateTime.UtcNow,
                TotalPrice = totalPrice,
                OrderItems = orderItems 
            };

            _context.Orders.Add(newOrder);
            
            await _context.SaveChangesAsync(ct);
            
            var kitchenTaskCommand = new CreateKitchenTaskCommand(newOrder.Id);
            var kitchenResult = await _mediator.Send(kitchenTaskCommand, ct);

            if (!kitchenResult.IsSuccess)
            {
                await transaction.RollbackAsync(ct);
                return Result.Failure<Guid>($"Failed to create kitchen task: {kitchenResult.Error}");
            }
            
            await transaction.CommitAsync(ct);
            
            return Result.Success(newOrder.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return Result.Failure<Guid>($"An unexpected error occurred: {ex.Message}");
        }
    }
}