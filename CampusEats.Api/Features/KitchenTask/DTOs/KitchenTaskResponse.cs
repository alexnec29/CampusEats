namespace CampusEats.Api.Features.KitchenTask.DTOs;

// Acestea sunt DTO-uri "interne" pentru a popula răspunsul principal
public record SimpleOrderItemResponse(string Name, int Quantity);
public record SimpleStaffResponse(Guid Id, string FullName);

public record KitchenTaskResponse(
    Guid Id,
    Guid OrderId,
    string Status,
    SimpleStaffResponse? AssignedStaff,
    List<SimpleOrderItemResponse> OrderItems,
    DateTime CreatedAt,
    DateTime? CompletedAt
);