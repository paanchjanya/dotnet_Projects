using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly BookstoreDbContext _context;

        public DashboardController(BookstoreDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalSales = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);

            var totalOrders = await _context.Orders.CountAsync();
            var totalBooks = await _context.Books.CountAsync();
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");

            // Category sales distribution
            var categorySales = await _context.OrderItems
                .Include(oi => oi.Book)
                .GroupBy(oi => oi.Book != null ? oi.Book.Category : "Unknown")
                .Select(g => new
                {
                    Category = g.Key,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .ToListAsync();

            // Recent orders
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status
                })
                .ToListAsync();

            return Ok(new
            {
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                TotalBooks = totalBooks,
                TotalCustomers = totalCustomers,
                CategorySales = categorySales,
                RecentOrders = recentOrders
            });
        }
    }
}
