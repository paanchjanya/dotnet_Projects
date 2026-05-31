using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeLeaveApi.Data;
using EmployeeLeaveApi.Models;

namespace EmployeeLeaveApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveBalancesController : ControllerBase
    {
        private readonly LeaveDbContext _context;

        public LeaveBalancesController(LeaveDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int id))
            {
                throw new UnauthorizedAccessException("User is not authorized.");
            }
            return id;
        }

        [HttpGet("my-balances")]
        public async Task<IActionResult> GetMyBalances()
        {
            var userId = GetCurrentUserId();
            var balances = await _context.LeaveBalances
                .Include(b => b.LeaveType)
                .Where(b => b.EmployeeId == userId)
                .ToListAsync();

            return Ok(balances);
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeBalances(int employeeId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null) return Unauthorized();

            // Employee can only see their own, Managers can see reportees, Admins see all
            if (currentUser.Role == "Employee" && currentUserId != employeeId)
            {
                return Forbid();
            }

            if (currentUser.Role == "Manager")
            {
                var employee = await _context.Users.FindAsync(employeeId);
                if (employee == null || employee.ManagerId != currentUserId)
                {
                    return Forbid();
                }
            }

            var balances = await _context.LeaveBalances
                .Include(b => b.LeaveType)
                .Where(b => b.EmployeeId == employeeId)
                .ToListAsync();

            return Ok(balances);
        }

        public class AdjustBalanceDto
        {
            public decimal AllocatedDays { get; set; }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("adjust/{id}")]
        public async Task<IActionResult> AdjustBalance(int id, [FromBody] AdjustBalanceDto dto)
        {
            var balance = await _context.LeaveBalances.FindAsync(id);
            if (balance == null) return NotFound(new { message = "Leave balance record not found" });

            balance.AllocatedDays = dto.AllocatedDays;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Balance adjusted successfully", balance });
        }
    }
}
