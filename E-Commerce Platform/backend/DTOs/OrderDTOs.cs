using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class OrderItemCreate
    {
        [Required]
        public int BookId { get; set; }
        
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }

    public class OrderCreate
    {
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required]
        [MinLength(1)]
        public List<OrderItemCreate> Items { get; set; } = new();
    }

    public class OrderItemResponse
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
