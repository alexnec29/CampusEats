using MediatR;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Features.Allergen.DTOs;

namespace CampusEats.Api.Features.Allergen;

public static class GetAllAllergens
{
    public record GetAllAllergensQuery : IRequest<IEnumerable<AllergenResponse>>;
}

public class GetAllAllergensHandler : IRequestHandler<GetAllAllergens.GetAllAllergensQuery, IEnumerable<AllergenResponse>>
{
    private readonly CampusEatsDbContext _context;

    public GetAllAllergensHandler(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AllergenResponse>> Handle(GetAllAllergens.GetAllAllergensQuery request, CancellationToken cancellationToken)
    {

        return await _context.Allergens
            .AsNoTracking()
            .Select(a => new AllergenResponse { Id = a.Id, Name = a.Name })
            .ToListAsync(cancellationToken);
    }
}