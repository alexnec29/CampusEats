using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using MediatR;

namespace CampusEats.Api.Features.User;

public class GetKitchenProfileByUserIdHandler(IKitchenProfileRepository kitchenProfileRepository) : IRequestHandler<GetKitchenProfileByUserIdRequest, IResult>
{
    public async Task<IResult> Handle(GetKitchenProfileByUserIdRequest request, CancellationToken cancellationToken)
    {
        KitchenProfile? kitchenProfile = await kitchenProfileRepository.GetByUserIdAsync(request.Id);
        if (kitchenProfile == null)
        {
            return Results.NotFound($"Kitchen profile not found for user with ID: {request.Id}");
        }

        GetKitchenProfileByUserIdResponse response = new GetKitchenProfileByUserIdResponse
        {
            CompanyName = kitchenProfile.CompanyName,
            KitchenAddress = kitchenProfile.KitchenAddress,
            WeeklyWorkingHours = kitchenProfile.WeeklyWorkingHours
        };
        
        return Results.Ok(response);
    }
}