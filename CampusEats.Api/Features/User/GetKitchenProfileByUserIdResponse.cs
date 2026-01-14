using CampusEats.Api.Models;

namespace CampusEats.Api.Features.User;

public class GetKitchenProfileByUserIdResponse
{
    public string CompanyName { get; set; } = string.Empty;
    public Address KitchenAddress { get; set; } = new Address();
    public WeeklyWorkingHours WeeklyWorkingHours { get; set; } = new WeeklyWorkingHours();
}