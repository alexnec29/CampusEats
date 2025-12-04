using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.User;

public class UpdateKitchenProfileHandler(IKitchenProfileRepository kitchenProfileRepository) : IRequestHandler<UpdateKitchenProfileRequest, IResult>
{
    public async Task<IResult> Handle(UpdateKitchenProfileRequest request, CancellationToken cancellationToken)
    {
        bool addFlag = false;
        KitchenProfile? kitchenProfile = await kitchenProfileRepository.GetByUserIdAsync(request.UserId);
        if (kitchenProfile == null)
        {
            addFlag = true;
            kitchenProfile = new KitchenProfile
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId
            };
        }
        
        kitchenProfile.CompanyName = request.CompanyName;
        kitchenProfile.KitchenAddress = request.KitchenAddress;
        kitchenProfile.WeeklyWorkingHours = request.WeeklyWorkingHours;

        if (addFlag)
        {
            await kitchenProfileRepository.AddAsync(kitchenProfile);
        }
        else
        {
            await kitchenProfileRepository.UpdateAsync(kitchenProfile);
        }
        
        return Results.NoContent();
    }
}