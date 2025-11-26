namespace CampusEats.Api.Models;

public class Jwt(string text, DateTime expires)
{
    public Guid Id { get; set; }
    public string Text { get; set; } = text;
    public DateTime Expires { get; set; } = expires;
}