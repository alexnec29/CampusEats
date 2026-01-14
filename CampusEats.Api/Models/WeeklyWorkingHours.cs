using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Models;

[Owned]
public class WeeklyWorkingHours
{
    public WorkingHours Monday { get; set; } = new WorkingHours();
    public WorkingHours Tuesday { get; set; } = new WorkingHours();
    public WorkingHours Wednesday { get; set; } = new WorkingHours();
    public WorkingHours Thursday { get; set; } = new WorkingHours();
    public WorkingHours Friday { get; set; } = new WorkingHours();
    public WorkingHours Saturday { get; set; } = new WorkingHours();
    public WorkingHours Sunday { get; set; } = new WorkingHours();
}