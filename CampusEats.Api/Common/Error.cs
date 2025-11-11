namespace CampusEats.Api.Common;

public record Error(string Code, string Message)
{
    public static Error None => new(string.Empty, string.Empty);
    public static Error NotFound(string entity) => new("NotFound", $"{entity} not found");
    public static Error Validation(string message) => new("Validation", message);
}