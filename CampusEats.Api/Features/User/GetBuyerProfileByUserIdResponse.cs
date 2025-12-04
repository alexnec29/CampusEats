using CampusEats.Api.Models;

namespace CampusEats.Api.Features.User;

public class GetBuyerProfileByUserIdResponse
{
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public int Age { get; set; }
    public Address DeliveryAddress { get; set; }
}