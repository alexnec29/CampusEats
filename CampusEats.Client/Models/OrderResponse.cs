using System;
using System.Collections.Generic;

namespace CampusEats.Client.Models
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }

    public class OrderItemResponse
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal MenuItemPrice { get; set; }
    }
    public enum OrderStatus
    {
        Inactive,
        Pending,
        Preparing,
        Ready,
        Completed,
        Cancelled
    }
}