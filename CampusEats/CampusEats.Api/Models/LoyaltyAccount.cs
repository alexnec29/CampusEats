using CampusEats.Api.Features.User;

namespace CampusEats.Api.Models;

public class LoyaltyAccount
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int PointsBalance { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
}