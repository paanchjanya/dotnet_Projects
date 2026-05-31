using Microsoft.EntityFrameworkCore;
using EmployeeLeaveApi.Models;
using EmployeeLeaveApi.Helpers;
using System;

namespace EmployeeLeaveApi.Data
{
    public class LeaveDbContext : DbContext
    {
        public LeaveDbContext(DbContextOptions<LeaveDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Self-Referencing Manager relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany(u => u.DirectReports)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure LeaveRequest relationships
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Employee)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.LeaveType)
                .WithMany()
                .HasForeignKey(lr => lr.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure LeaveBalance relationships
            modelBuilder.Entity<LeaveBalance>()
                .HasOne(lb => lb.Employee)
                .WithMany(u => u.LeaveBalances)
                .HasForeignKey(lb => lb.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveBalance>()
                .HasOne(lb => lb.LeaveType)
                .WithMany()
                .HasForeignKey(lb => lb.LeaveTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Seed Leave Types
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { Id = 1, Name = "Sick Leave", DefaultDays = 10, RequiresAttachment = true },
                new LeaveType { Id = 2, Name = "Casual Leave", DefaultDays = 12, RequiresAttachment = false },
                new LeaveType { Id = 3, Name = "Annual Leave", DefaultDays = 20, RequiresAttachment = false },
                new LeaveType { Id = 4, Name = "Maternity Leave", DefaultDays = 90, RequiresAttachment = true },
                new LeaveType { Id = 5, Name = "Paternity Leave", DefaultDays = 15, RequiresAttachment = true }
            );

            // Seed Users
            string adminHash = SecurityHelper.HashPassword("Admin123!");
            string managerHash = SecurityHelper.HashPassword("Bob123!");
            string employee1Hash = SecurityHelper.HashPassword("Alice123!");
            string employee2Hash = SecurityHelper.HashPassword("Charlie123!");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = adminHash,
                    Email = "admin@company.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Department = "HR & IT",
                    Role = "Admin",
                    JoinDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "bob",
                    PasswordHash = managerHash,
                    Email = "bob@company.com",
                    FirstName = "Bob",
                    LastName = "Manager",
                    Department = "Engineering",
                    Role = "Manager",
                    JoinDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 3,
                    Username = "alice",
                    PasswordHash = employee1Hash,
                    Email = "alice@company.com",
                    FirstName = "Alice",
                    LastName = "Smith",
                    Department = "Engineering",
                    Role = "Employee",
                    ManagerId = 2,
                    JoinDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 4,
                    Username = "charlie",
                    PasswordHash = employee2Hash,
                    Email = "charlie@company.com",
                    FirstName = "Charlie",
                    LastName = "Brown",
                    Department = "Engineering",
                    Role = "Employee",
                    ManagerId = 2,
                    JoinDate = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Leave Balances for Employees and Managers
            modelBuilder.Entity<LeaveBalance>().HasData(
                // Bob (Manager)
                new LeaveBalance { Id = 1, EmployeeId = 2, LeaveTypeId = 1, AllocatedDays = 10, UsedDays = 0, PendingDays = 0 },
                new LeaveBalance { Id = 2, EmployeeId = 2, LeaveTypeId = 2, AllocatedDays = 12, UsedDays = 0, PendingDays = 0 },
                new LeaveBalance { Id = 3, EmployeeId = 2, LeaveTypeId = 3, AllocatedDays = 20, UsedDays = 0, PendingDays = 0 },
                
                // Alice
                new LeaveBalance { Id = 4, EmployeeId = 3, LeaveTypeId = 1, AllocatedDays = 10, UsedDays = 0, PendingDays = 0 },
                new LeaveBalance { Id = 5, EmployeeId = 3, LeaveTypeId = 2, AllocatedDays = 12, UsedDays = 0, PendingDays = 0 },
                new LeaveBalance { Id = 6, EmployeeId = 3, LeaveTypeId = 3, AllocatedDays = 20, UsedDays = 0, PendingDays = 0 },
                
                // Charlie
                new LeaveBalance { Id = 7, EmployeeId = 4, LeaveTypeId = 1, AllocatedDays = 10, UsedDays = 0, PendingDays = 0 },
                new LeaveBalance { Id = 8, EmployeeId = 4, LeaveTypeId = 2, AllocatedDays = 12, UsedDays = 0, PendingDays = 0 },
                new LeaveBalance { Id = 9, EmployeeId = 4, LeaveTypeId = 3, AllocatedDays = 20, UsedDays = 0, PendingDays = 0 }
            );
        }
    }
}
