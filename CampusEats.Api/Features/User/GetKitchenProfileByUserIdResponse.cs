using CampusEats.Api.Models;

namespace CampusEats.Api.Features.User;

public class GetKitchenProfileByUserIdResponse
{
    public string CompanyName { get; set; }
    public Address KithcenAddress { get; set; }
    public WeeklyWorkingHours WeeklyWorkingHours { get; set; }
}