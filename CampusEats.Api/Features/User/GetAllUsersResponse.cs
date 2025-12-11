namespace CampusEats.Api.Features.User;

public class GetAllUsersResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
}