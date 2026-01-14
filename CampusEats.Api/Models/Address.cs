using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Models;

[Owned]
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
}