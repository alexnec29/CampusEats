using System.Net.Mime;
using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Features.Allergen.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Endpoints;

public static class AllergenEndpoints
{
    public static void MapAllergenEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/allergens")
            .WithTags("Allergens") 
            .WithOpenApi();

        group.MapGet("/{id:int}", (int id, IMediator mediator) => 
        {
            return Results.Ok($"Allergen {id} fetched (GetById logic pending)"); 
        }).WithName("GetByIdAllergen")
          .RequireAuthorization("AllRoles");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllAllergens.GetAllAllergensQuery());
            return Results.Ok(result); 
        }).WithName("GetAllAllergens")
          .RequireAuthorization("AllRoles");

        group.MapPost("/", async ([FromBody] CreateAllergenRequest request, IMediator mediator) =>
        {
            var command = new CreateAllergen.CreateAllergenCommand(request.Name);
            
            try
            {
                var result = await mediator.Send(command);
                return Results.CreatedAtRoute("GetByIdAllergen", new { id = result.Id }, result);
            }
            catch (Exception ex) when (ex.Message.Contains("already exists"))
            {

                return Results.Conflict(ex.Message); 
            }
        })
        .Accepts<CreateAllergenRequest>(MediaTypeNames.Application.Json)
        .WithName("CreateAllergen")
        .RequireAuthorization("Kitchen");
        group.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteAllergen.DeleteAllergenCommand(id));
                return Results.NoContent(); 
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(); 
            }
        }).WithName("DeleteAllergen")
          .RequireAuthorization("Kitchen");
    }
}