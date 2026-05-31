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
    public class LeaveTypesController : ControllerBase
    {
        private readonly LeaveDbContext _context;

        public LeaveTypesController(LeaveDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaveTypes()
        {
            var types = await _context.LeaveTypes.ToListAsync();
            return Ok(types);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateLeaveType([FromBody] LeaveType type)
        {
            _context.LeaveTypes.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLeaveTypes), new { id = type.Id }, type);
        }
    }
}
