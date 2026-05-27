using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace CineBooking.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var statusCode = StatusCodes.Status500InternalServerError;
            var message = "An unexpected error occurred.";

            // Handle concurrency or duplicate key exceptions
            if (exception is DbUpdateException dbUpdateEx)
            {
                if (dbUpdateEx.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    statusCode = StatusCodes.Status409Conflict;
                    message = "Seat Already Reserved";
                }
                else
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "Database update error.";
                }
            }
            else if (exception is InvalidOperationException invEx && invEx.Message == "Seat Already Reserved")
            {
                statusCode = StatusCodes.Status409Conflict;
                message = "Seat Already Reserved";
            }
            else if (exception is InvalidOperationException balanceEx && balanceEx.Message == "Insufficient balance")
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = "Insufficient balance";
            }

            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(result);
        }
    }
}
