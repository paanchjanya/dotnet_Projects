using System;
using System.Collections.Generic;

namespace frontend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
    }

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class BookCreateUpdate
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class CartItem
    {
        public Book Book { get; set; } = new();
        public int Quantity { get; set; }
    }

    public class OrderItemCreate
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderCreate
    {
        public string ShippingAddress { get; set; } = string.Empty;
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

    public class DashboardStats
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalBooks { get; set; }
        public int TotalCustomers { get; set; }
        public List<CategorySalesStats> CategorySales { get; set; } = new();
        public List<RecentOrderStats> RecentOrders { get; set; } = new();
    }

    public class CategorySalesStats
    {
        public string Category { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentOrderStats
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
