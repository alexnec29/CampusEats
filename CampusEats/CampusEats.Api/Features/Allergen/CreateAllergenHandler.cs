using MediatR;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Models; 
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Features.Allergen.DTOs;

namespace CampusEats.Api.Features.Allergen;

public static class CreateAllergen
{
    public record CreateAllergenCommand(string Name) : IRequest<AllergenResponse>;
}


public class CreateAllergenHandler : IRequestHandler<CreateAllergen.CreateAllergenCommand, AllergenResponse>
{
    private readonly CampusEatsDbContext _context;

    public CreateAllergenHandler(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task<AllergenResponse> Handle(CreateAllergen.CreateAllergenCommand request, CancellationToken cancellationToken)
    {
        
        if (await _context.Allergens.AnyAsync(a => a.Name.ToLower() == request.Name.ToLower(), cancellationToken))
        {
            
            throw new Exception("Allergen with this name already exists.");
        }

        
        var allergen = new Models.Allergen { Name = request.Name };

        
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync(cancellationToken);

        return new AllergenResponse { Id = allergen.Id, Name = allergen.Name };
    }
}

