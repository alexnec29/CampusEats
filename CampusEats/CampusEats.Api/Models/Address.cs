using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Models;

[Owned]
public class Address
{
    public string street { get; set; }
    public string building { get; set; }
    public string city { get; set; }
    public string county { get; set; }
}