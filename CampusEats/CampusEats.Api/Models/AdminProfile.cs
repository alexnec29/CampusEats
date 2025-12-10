using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEats.Api.Models;

[Table("admin_profile")]
public class AdminProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}