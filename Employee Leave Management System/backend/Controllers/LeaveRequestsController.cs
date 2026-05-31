using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeLeaveApi.Data;
using EmployeeLeaveApi.Models;
using EmployeeLeaveApi.Services;

namespace EmployeeLeaveApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly LeaveDbContext _context;
        private readonly IEmailService _emailService;

        public LeaveRequestsController(LeaveDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        public class SubmitLeaveDto
        {
            public int LeaveTypeId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        public class ActionLeaveDto
        {
            public string Status { get; set; } = string.Empty; // Approved, Rejected
            public string? Remarks { get; set; }
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromForm] SubmitLeaveDto dto, IFormFile? file)
        {
            try
            {
                var employeeId = GetCurrentUserId();
                var employee = await _context.Users
                    .Include(u => u.Manager)
                    .FirstOrDefaultAsync(u => u.Id == employeeId);

                if (employee == null) return NotFound(new { message = "Employee not found" });

                if (dto.StartDate.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(new { message = "Start date cannot be in the past" });
                }

                if (dto.EndDate.Date < dto.StartDate.Date)
                {
                    return BadRequest(new { message = "End date must be on or after start date" });
                }

                var leaveType = await _context.LeaveTypes.FindAsync(dto.LeaveTypeId);
                if (leaveType == null) return NotFound(new { message = "Leave type not found" });

                // Calculate days (e.g. 1 day minimum, includes both start and end days)
                decimal totalDays = (decimal)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;

                // Validate Leave Balance
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == dto.LeaveTypeId);

                if (balance == null)
                {
                    return BadRequest(new { message = "Leave balance not initialized for this leave type" });
                }

                decimal availableDays = balance.AllocatedDays - balance.UsedDays - balance.PendingDays;
                if (totalDays > availableDays)
                {
                    return BadRequest(new { message = $"Insufficient leave balance. Requested: {totalDays} days, Available: {availableDays} days" });
                }

                // Check attachment requirement
                string? attachmentPath = null;
                string? attachmentFileName = null;

                if (leaveType.RequiresAttachment)
                {
                    if (file == null || file.Length == 0)
                    {
                        return BadRequest(new { message = $"An attachment (PDF or DOCX) is required for {leaveType.Name}." });
                    }

                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (extension != ".pdf" && extension != ".docx" && extension != ".doc")
                    {
                        return BadRequest(new { message = "Only PDF, DOC, and DOCX files are allowed for leave attachments." });
                    }

                    var folderName = Path.Combine("wwwroot", "uploads");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    if (!Directory.Exists(pathToSave))
                    {
                        Directory.CreateDirectory(pathToSave);
                    }

                    var fileName = $"attachment_{employeeId}_{DateTime.UtcNow.Ticks}{extension}";
                    var fullPath = Path.Combine(pathToSave, fileName);
                    
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    attachmentPath = $"/uploads/{fileName}";
                    attachmentFileName = file.FileName;
                }

                // Create leave request
                var request = new LeaveRequest
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = dto.LeaveTypeId,
                    StartDate = dto.StartDate.Date,
                    EndDate = dto.EndDate.Date,
                    TotalDays = totalDays,
                    Reason = dto.Reason,
                    Status = "Pending",
                    AttachmentPath = attachmentPath,
                    AttachmentFileName = attachmentFileName,
                    AppliedDate = DateTime.UtcNow
                };

                // Deduct from balance (mark as pending)
                balance.PendingDays += totalDays;

                _context.LeaveRequests.Add(request);
                await _context.SaveChangesAsync();

                // Notification to employee
                _context.Notifications.Add(new Notification
                {
                    UserId = employeeId,
                    Message = $"Your leave request for {totalDays} days of {leaveType.Name} has been submitted successfully.",
                    CreatedAt = DateTime.UtcNow
                });

                // Manager Notifications & Email
                if (employee.ManagerId.HasValue)
                {
                    var manager = await _context.Users.FindAsync(employee.ManagerId.Value);
                    if (manager != null)
                    {
                        // In-app notification for manager
                        _context.Notifications.Add(new Notification
                        {
                            UserId = manager.Id,
                            Message = $"New leave request submitted by {employee.FirstName} {employee.LastName} ({totalDays} days of {leaveType.Name}).",
                            CreatedAt = DateTime.UtcNow
                        });

                        // Simulated email
                        string emailBody = $"Hello {manager.FirstName},\n\n{employee.FirstName} {employee.LastName} has submitted a new leave request for {leaveType.Name}.\n" +
                            $"Duration: {dto.StartDate.ToShortDateString()} to {dto.EndDate.ToShortDateString()} ({totalDays} days).\n" +
                            $"Reason: {dto.Reason}\n\n" +
                            $"Please log into the system to approve or reject this request.";
                        
                        await _emailService.SendEmailAsync(manager.Email, $"New Leave Request - {employee.FirstName} {employee.LastName}", emailBody);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Leave request submitted successfully", request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetCurrentUserId();
            var requests = await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.EmployeeId == userId)
                .OrderByDescending(lr => lr.AppliedDate)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("pending-approvals")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var userId = GetCurrentUserId();
            
            // Managers can view pending requests of employees who report to them
            var requests = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.Employee!.ManagerId == userId && lr.Status == "Pending")
                .OrderBy(lr => lr.AppliedDate)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("all-requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var userId = GetCurrentUserId();
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return Unauthorized();

            // Admins can see all, Managers can see all for their team, Employees see only their own
            IQueryable<LeaveRequest> query = _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType);

            if (currentUser.Role == "Manager")
            {
                query = query.Where(lr => lr.Employee!.ManagerId == userId || lr.EmployeeId == userId);
            }
            else if (currentUser.Role == "Employee")
            {
                query = query.Where(lr => lr.EmployeeId == userId);
            }

            var requests = await query.OrderByDescending(lr => lr.AppliedDate).ToListAsync();
            return Ok(requests);
        }

        [HttpPost("action/{id}")]
        public async Task<IActionResult> ActionRequest(int id, [FromBody] ActionLeaveDto dto)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var manager = await _context.Users.FindAsync(managerId);
                if (manager == null) return Unauthorized();

                var request = await _context.LeaveRequests
                    .Include(lr => lr.Employee)
                    .Include(lr => lr.LeaveType)
                    .FirstOrDefaultAsync(lr => lr.Id == id);

                if (request == null) return NotFound(new { message = "Leave request not found" });

                if (request.Status != "Pending")
                {
                    return BadRequest(new { message = "Leave request has already been actioned" });
                }

                // Verify that this manager is authorized to approve this request (or admin)
                if (request.Employee!.ManagerId != managerId && manager.Role != "Admin")
                {
                    return Forbid();
                }

                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.EmployeeId == request.EmployeeId && b.LeaveTypeId == request.LeaveTypeId);

                if (balance == null)
                {
                    return BadRequest(new { message = "Leave balance not found for this employee" });
                }

                if (dto.Status == "Approved")
                {
                    request.Status = "Approved";
                    balance.PendingDays -= request.TotalDays;
                    balance.UsedDays += request.TotalDays;
                }
                else if (dto.Status == "Rejected")
                {
                    request.Status = "Rejected";
                    balance.PendingDays -= request.TotalDays;
                }
                else
                {
                    return BadRequest(new { message = "Invalid status. Use 'Approved' or 'Rejected'." });
                }

                request.ManagerRemarks = dto.Remarks;
                request.ActionedDate = DateTime.UtcNow;
                request.ApprovedByManagerId = managerId;

                // Add in-app notification for employee
                _context.Notifications.Add(new Notification
                {
                    UserId = request.EmployeeId,
                    Message = $"Your leave request for {request.TotalDays} days of {request.LeaveType!.Name} has been {dto.Status.ToLower()} by your manager.",
                    CreatedAt = DateTime.UtcNow
                });

                // Simulated Email notification to Employee
                string emailBody = $"Hello {request.Employee.FirstName},\n\nYour leave request for {request.LeaveType.Name} ({request.StartDate.ToShortDateString()} to {request.EndDate.ToShortDateString()}) has been {dto.Status.ToUpper()}.\n" +
                    $"Manager Remarks: {dto.Remarks ?? "None"}\n\n" +
                    $"Regards,\nCompany Leave Management System";

                await _emailService.SendEmailAsync(request.Employee.Email, $"Leave Request {dto.Status} - Notification", emailBody);

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Leave request has been {dto.Status.ToLower()} successfully", request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelRequest(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var request = await _context.LeaveRequests
                    .Include(lr => lr.Employee)
                    .Include(lr => lr.LeaveType)
                    .FirstOrDefaultAsync(lr => lr.Id == id);

                if (request == null) return NotFound(new { message = "Leave request not found" });

                if (request.EmployeeId != userId)
                {
                    return Forbid();
                }

                if (request.Status != "Pending")
                {
                    return BadRequest(new { message = "Only pending leave requests can be cancelled" });
                }

                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.EmployeeId == userId && b.LeaveTypeId == request.LeaveTypeId);

                if (balance != null)
                {
                    balance.PendingDays -= request.TotalDays;
                }

                request.Status = "Cancelled";
                request.ActionedDate = DateTime.UtcNow;

                // Delete uploaded file to clean up if cancelled and file exists
                if (!string.IsNullOrEmpty(request.AttachmentPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", request.AttachmentPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    request.AttachmentPath = null;
                    request.AttachmentFileName = null;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Leave request cancelled successfully", request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
