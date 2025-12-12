namespace CampusEats.Api.Features.User;

public class ChangePasswordResponse
{
    public string Message { get; set; }

    public ChangePasswordResponse(string message)
    {
        Message = message;
    }
}