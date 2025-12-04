using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using MediatR;

namespace CampusEats.Api.Features.User;

public class GetBuyerProfileByUserIdHandler(IBuyerProfileRepository buyerProfileRepository) : IRequestHandler<GetBuyerProfileByUserIdRequest, IResult>
{
    public async Task<IResult> Handle(GetBuyerProfileByUserIdRequest request, CancellationToken cancellationToken)
    {
        BuyerProfile? buyerProfile = await buyerProfileRepository.GetByUserIdAsync(request.Id);
        if (buyerProfile == null)
        {
            return Results.NotFound($"Buyer profile not found for user with ID: {request.Id}");
        }

        GetBuyerProfileByUserIdResponse byUserIdResponse = new GetBuyerProfileByUserIdResponse
        {
            LastName = buyerProfile.LastName,
            FirstName = buyerProfile.FirstName,
            Age = buyerProfile.Age,
            DeliveryAddress = buyerProfile.DeliveryAddress
        };
        
        return Results.Ok(byUserIdResponse);
    }
}