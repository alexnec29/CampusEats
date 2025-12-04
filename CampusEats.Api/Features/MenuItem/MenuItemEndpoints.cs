using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using CampusEats.Api.Features.MenuItem.DTOs;
using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Models.Enums; 

namespace CampusEats.Api.Features.MenuItem;

public static class MenuItemEndpoints
{
    public static void MapMenuItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/menu")
                       .WithTags("MenuItem");

        group.MapPost("", async (CreateMenuItemRequest request, IMediator mediator) =>
        {
            var command = new CreateMenuItem.CreateMenuItemCommand(
                request.Name, 
                request.Description, 
                request.Price, 
                request.CategoryId, 
                request.AllergenIds ?? new List<int>());

            var result = await mediator.Send(command);
            return Results.Created($"/api/menu/{result.Id}", result);
        }).WithName("CreateMenuItem");

        group.MapPut("/{id:int}", async (int id, UpdateMenuItemRequest request, IMediator mediator) =>
        {
            if (id != request.Id)
            {
                return Results.BadRequest("ID-ul din rută nu se potrivește cu ID-ul din corp.");
            }

            var command = new UpdateMenuItem.UpdateMenuItemCommand(
                request.Id,
                request.Name,
                request.Description,
                request.Price,
                request.IsAvailable,
                request.AllergenIds ?? new List<int>()); 
            
            try
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }

        }).WithName("UpdateMenuItem");

        
        group.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            var command = new DeleteMenuItem.DeleteMenuItemCommand(id);
            
            try
            {
                await mediator.Send(command);
                return Results.NoContent(); 
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        }).WithName("DeleteMenuItem");

    
        group.MapGet("", async (IMediator mediator, int pageNumber = 1, int pageSize = 10) =>
        {
            var query = new GetAllMenuItems.Query(pageNumber, pageSize);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).WithName("GetAllMenuItems");

    
        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var query = new GetMenuItemById.Query(id);
            
            try
            {
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        }).WithName("GetMenuItemById");

        group.MapGet("/category/{category}", async (MenuCategory category, IMediator mediator) =>
        {
            var query = new GetMenuItemsByCategory.Query(category);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).WithName("GetMenuItemsByCategory");
        
    
        group.MapPost("/{id:int}/allergens/{allergenId:int}", async (int id, int allergenId, IMediator mediator) =>
        {
            var command = new AddAllergenToMenuItem.AddAllergenToMenuItemCommand(id, allergenId);
            
            try
            {
                await mediator.Send(command);
                return Results.NoContent(); 
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        }).WithName("AddAllergenToMenuItem");

        
        group.MapDelete("/{id:int}/allergens/{allergenId:int}", async (int id, int allergenId, IMediator mediator) =>
        {
            var command = new RemoveAllergenFromMenuItem.RemoveAllergenFromMenuItemCommand(id, allergenId);
            
            try
            {
                await mediator.Send(command);
                return Results.NoContent(); 
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        }).WithName("RemoveAllergenFromMenuItem");
    }
}