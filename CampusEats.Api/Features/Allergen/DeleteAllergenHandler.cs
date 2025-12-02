using MediatR;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Infrastructure;
namespace CampusEats.Api.Features.Allergen;

public static class DeleteAllergen
{
        public record DeleteAllergenCommand(int Id) : IRequest<Unit>; 
}


public class DeleteAllergenHandler : IRequestHandler<DeleteAllergen.DeleteAllergenCommand, Unit>
{
    private readonly CampusEatsDbContext _context;

    public DeleteAllergenHandler(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteAllergen.DeleteAllergenCommand request, CancellationToken cancellationToken)
    {
        
        var allergen = await _context.Allergens.FindAsync(new object[] { request.Id }, cancellationToken);

        if (allergen == null)
        {
            throw new KeyNotFoundException($"Allergen with Id {request.Id} not found.");
        }
        _context.Allergens.Remove(allergen);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}