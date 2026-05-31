using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeLeaveApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    AllocatedDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PendingDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByManagerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_ApprovedByManagerId",
                        column: x => x.ApprovedByManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "Id", "DefaultDays", "Name", "RequiresAttachment" },
                values: new object[,]
                {
                    { 1, 10m, "Sick Leave", true },
                    { 2, 12m, "Casual Leave", false },
                    { 3, 20m, "Annual Leave", false },
                    { 4, 90m, "Maternity Leave", true },
                    { 5, 15m, "Paternity Leave", true }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Department", "Email", "FirstName", "JoinDate", "LastName", "ManagerId", "PasswordHash", "ProfilePictureUrl", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "HR & IT", "admin@company.com", "System", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrator", null, "PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=", null, "Admin", "admin" },
                    { 2, "Engineering", "bob@company.com", "Bob", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manager", null, "ST/ct5DjWan4v64mDLjfEqsMJIU+E9TZtiOcM9e3JYo=", null, "Manager", "bob" }
                });

            migrationBuilder.InsertData(
                table: "LeaveBalances",
                columns: new[] { "Id", "AllocatedDays", "EmployeeId", "LeaveTypeId", "PendingDays", "UsedDays" },
                values: new object[,]
                {
                    { 1, 10m, 2, 1, 0m, 0m },
                    { 2, 12m, 2, 2, 0m, 0m },
                    { 3, 20m, 2, 3, 0m, 0m }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Department", "Email", "FirstName", "JoinDate", "LastName", "ManagerId", "PasswordHash", "ProfilePictureUrl", "Role", "Username" },
                values: new object[,]
                {
                    { 3, "Engineering", "alice@company.com", "Alice", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Smith", 2, "g89+N06D2i7m7NylitWyi6OPzlaL+dKhuceunMH+nnw=", null, "Employee", "alice" },
                    { 4, "Engineering", "charlie@company.com", "Charlie", new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Brown", 2, "1V382PF6ecIMos83+nLQ/mhzsbWAb+f0m0QZmmoTWno=", null, "Employee", "charlie" }
                });

            migrationBuilder.InsertData(
                table: "LeaveBalances",
                columns: new[] { "Id", "AllocatedDays", "EmployeeId", "LeaveTypeId", "PendingDays", "UsedDays" },
                values: new object[,]
                {
                    { 4, 10m, 3, 1, 0m, 0m },
                    { 5, 12m, 3, 2, 0m, 0m },
                    { 6, 20m, 3, 3, 0m, 0m },
                    { 7, 10m, 4, 1, 0m, 0m },
                    { 8, 12m, 4, 2, 0m, 0m },
                    { 9, 20m, 4, 3, 0m, 0m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_LeaveTypeId",
                table: "LeaveBalances",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApprovedByManagerId",
                table: "LeaveRequests",
                column: "ApprovedByManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerId",
                table: "Users",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveBalances");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
