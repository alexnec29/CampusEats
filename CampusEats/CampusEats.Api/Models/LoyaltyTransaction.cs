namespace CampusEats.Api.Models;

public class LoyaltyTransaction
{
    public int Id { get; set; }
    public int LoyaltyAccountId { get; set; }
    public int Points { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;
}