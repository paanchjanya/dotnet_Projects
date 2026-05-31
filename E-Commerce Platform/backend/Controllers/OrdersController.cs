using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly BookstoreDbContext _context;

        public OrdersController(BookstoreDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreate dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { Message = "User identity not found." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var book = await _context.Books.FindAsync(itemDto.BookId);
                if (book == null)
                {
                    return BadRequest(new { Message = $"Book with ID {itemDto.BookId} not found." });
                }

                if (book.StockQuantity < itemDto.Quantity)
                {
                    return BadRequest(new { Message = $"Insufficient stock for book '{book.Title}'. Available: {book.StockQuantity}, Requested: {itemDto.Quantity}" });
                }

                // Deduct stock
                book.StockQuantity -= itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    BookId = book.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = book.Price
                };

                orderItems.Add(orderItem);
                totalAmount += book.Price * itemDto.Quantity;
            }

            var order = new Order
            {
                UserId = userId,
                CustomerName = user.Username,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                ShippingAddress = dto.ShippingAddress,
                Status = "Pending",
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order placed successfully.", OrderId = order.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => MapToOrderResponse(o))
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { Message = "User identity not found." });
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => MapToOrderResponse(o))
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { Message = "Order not found." });
            }

            var validStatuses = new[] { "Pending", "Shipped", "Delivered" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest(new { Message = "Invalid order status. Allowed: Pending, Shipped, Delivered." });
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order status updated successfully." });
        }

        private static OrderResponse MapToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    BookId = oi.BookId,
                    BookTitle = oi.Book?.Title ?? "Unknown Book",
                    BookAuthor = oi.Book?.Author ?? "Unknown Author",
                    CoverImageUrl = oi.Book?.CoverImageUrl ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}
