using System.ComponentModel.DataAnnotations.Schema;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Models;

[Table("users")]
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public IList<Order> Orders { get; set; } = new List<Order>();
}