using System.ComponentModel.DataAnnotations.Schema;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Models;

[Table("users")]
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string HashedPassword { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
    public IList<Order> Orders { get; set; } = new List<Order>();
}