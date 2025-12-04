using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.User;

public class GetUserByIdResponse
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}