using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Models;

[Owned]
public class WorkingHours
{
    public TimeSpan Open { get; set; }
    public TimeSpan Close { get; set; }
}