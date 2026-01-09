using MediatR;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Features.Allergen.DTOs;

namespace CampusEats.Api.Features.Allergen;

public static class GetAllergenById
{
    public record GetAllergenByIdQuery(int Id) : IRequest<AllergenResponse?>;
}

public class GetAllergenByIdHandler : IRequestHandler<GetAllergenById.GetAllergenByIdQuery, AllergenResponse?>
{
    private readonly CampusEatsDbContext _context;

    public GetAllergenByIdHandler(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task<AllergenResponse?> Handle(GetAllergenById.GetAllergenByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Allergens
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new AllergenResponse { Id = a.Id, Name = a.Name })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
