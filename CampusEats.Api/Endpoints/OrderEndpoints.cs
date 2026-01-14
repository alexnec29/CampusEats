using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Features.Order.AddOrderItem;
using CampusEats.Api.Features.Order.RemoveOrderItem;
using CampusEats.Api.Features.Order.UpdateOrderStatus;
using CampusEats.Api.Features.Order.GetOrderById;
using CampusEats.Api.Features.Order.GetAllOrders;
using CampusEats.Api.Features.Order.GetOrdersByStatus;
using CampusEats.Api.Features.Order.GetUserOrders;
using CampusEats.Api.Features.Order.CancelOrder;
using CampusEats.Api.Features.Order.CancelOrderByKitchen;
using CampusEats.Api.Features.Order.UpdateOrderItemQuantity;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // Group all order endpoints and tag for Swagger
        var orders = app.MapGroup("api/orders")
                        .WithTags("Orders")
                        .WithOpenApi();

        // Create order
        orders.MapPost("/", async (CreateOrderRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdString = httpContext.User.FindFirst("/id")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }
            var command = request with { UserId = userId };
            return await mediator.Send(command);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Add item to order
        orders.MapPost("/{orderId}/items", async (int orderId, AddOrderItemRequest request, IMediator mediator) =>
        {
            var command = request with { OrderId = orderId };
            return await mediator.Send(command);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));
        
        // Update item (quantity)
        orders.MapPut("/{orderId}/items/{itemId}", async (int orderId, int itemId, UpdateOrderItemQuantityRequest request, IMediator mediator) =>
        {
            var command = request with { OrderId = orderId, OrderItemId = itemId };
            return await mediator.Send(command);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Remove item
        orders.MapDelete("/{orderId}/items/{itemId}", async (int orderId, int itemId, IMediator mediator) =>
        {
            var command = new RemoveOrderItemRequest(orderId, itemId);
            return await mediator.Send(command);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Update order status
        orders.MapPut("/{orderId}/status", async (int orderId, UpdateOrderStatusRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userRole = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Buyer can only set status to Placed, Cancelled or Paid
            if (userRole == nameof(CampusEats.Api.Models.Enums.Role.Buyer) && 
                request.Status != CampusEats.Api.Models.Enums.OrderStatus.Placed && 
                request.Status != CampusEats.Api.Models.Enums.OrderStatus.Cancelled &&
                request.Status != CampusEats.Api.Models.Enums.OrderStatus.Paid)
            {
                return Results.Forbid();
            }

            // Kitchen can only set status to Preparing, Ready, Completed or Cancelled
            if (userRole == nameof(CampusEats.Api.Models.Enums.Role.Kitchen) &&
                request.Status != CampusEats.Api.Models.Enums.OrderStatus.Preparing &&
                request.Status != CampusEats.Api.Models.Enums.OrderStatus.Ready &&
                request.Status != CampusEats.Api.Models.Enums.OrderStatus.Completed &&
                request.Status != CampusEats.Api.Models.Enums.OrderStatus.Cancelled)
            {
                return Results.Forbid();
            }

            var command = request with { OrderId = orderId };
            return await mediator.Send(command);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Cancel order
        orders.MapPost("/{orderId}/cancel", async (int orderId, IMediator mediator) =>
        {
            var command = new CancelOrderRequest(orderId);
            return await mediator.Send(command);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Get order by ID
        orders.MapGet("/{orderId}", async (int orderId, IMediator mediator) =>
        {
            var query = new GetOrderByIdRequest(orderId);
            return await mediator.Send(query);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Get all orders
        orders.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetAllOrdersRequest();
            return await mediator.Send(query);
        }).RequireAuthorization(nameof(AuthorizationPolicies.AdminPolicy));

        // Get orders by status
        orders.MapGet("/status", async (string status, IMediator mediator) =>
        {
            if (!Enum.TryParse<CampusEats.Api.Models.Enums.OrderStatus>(status, true, out var parsedStatus))
                return Results.BadRequest("Invalid order status");

            var query = new GetOrdersByStatusRequest(parsedStatus);
            return await mediator.Send(query);
        }).RequireAuthorization(nameof(AuthorizationPolicies.KitchenPolicy));

        // Get orders by user
        orders.MapGet("/user/{userId:guid}", async (Guid userId, IMediator mediator) =>
        {
            var query = new GetUserOrdersRequest(userId);
            return await mediator.Send(query);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Get my orders
        orders.MapGet("/my-orders", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userIdString = httpContext.User.FindFirst("/id")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }
            var query = new GetUserOrdersRequest(userId);
            return await mediator.Send(query);
        });

        orders.MapPost("/cancel-by-kitchen", async (CancelOrderByKitchenRequest request, IMediator mediator) => 
            await mediator.Send(request)).RequireAuthorization(nameof(AuthorizationPolicies.KitchenPolicy));
    }
}
