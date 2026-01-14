using CampusEats.Api.Models;

namespace CampusEats.Api.Features.User;

public class GetBuyerProfileByUserIdResponse
{
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public int Age { get; set; }
    public Address DeliveryAddress { get; set; } = new Address();
}