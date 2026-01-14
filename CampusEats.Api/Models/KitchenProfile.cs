using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEats.Api.Models;

[Table("kitchen_profile")]
public class KitchenProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Address KitchenAddress { get; set; } = new Address();
    public WeeklyWorkingHours WeeklyWorkingHours { get; set; } = new WeeklyWorkingHours();
}