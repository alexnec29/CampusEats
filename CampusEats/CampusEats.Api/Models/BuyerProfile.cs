using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEats.Api.Models;

[Table("buyer_profile")]
public class BuyerProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public int Age { get; set; }
    public Address DeliveryAddress { get; set; }
}