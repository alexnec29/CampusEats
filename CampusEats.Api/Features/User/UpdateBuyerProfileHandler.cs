using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.User;

public class UpdateBuyerProfileHandler(IBuyerProfileRepository buyerProfileRepository) : IRequestHandler<UpdateBuyerProfileRequest, IResult>
{
    public async Task<IResult> Handle(UpdateBuyerProfileRequest request, CancellationToken cancellationToken)
    {
        bool addFlag = false;
        BuyerProfile? buyerProfile = await buyerProfileRepository.GetByUserIdAsync(request.UserId);
        if (buyerProfile == null)
        {
            addFlag = true;
            buyerProfile = new BuyerProfile
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId
            };
        }
        
        buyerProfile.LastName = request.LastName;
        buyerProfile.FirstName = request.FirstName;
        buyerProfile.Age = request.Age;
        buyerProfile.DeliveryAddress = request.DeliveryAddress;

        if (addFlag)
        {
            await buyerProfileRepository.AddAsync(buyerProfile);
        }
        else
        {
            await buyerProfileRepository.UpdateAsync(buyerProfile);
        }
        
        return Results.NoContent();
    }
}