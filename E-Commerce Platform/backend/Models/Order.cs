using System;
using System.Collections.Generic;

namespace Backend.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
