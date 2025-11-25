using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEats.Api.Models;

[Table("kitchen_profile")]
public class KitchenProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CompanyName { get; set; }
    public Address KitchenAddress { get; set; }
    public WeeklyWorkingHours WeeklyWorkingHours { get; set; }
}