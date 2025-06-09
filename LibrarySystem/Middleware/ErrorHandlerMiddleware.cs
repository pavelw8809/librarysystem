using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace LibrarySystem.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public ErrorHandlerMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _requestDelegate(context);
            }
            catch (SqlException ex)
            {
                await HandleExceptionAsync(context, ex, "Database SQL Connection Error", HttpStatusCode.InternalServerError);
            }
            catch (DbUpdateException ex) when (ex is DbUpdateConcurrencyException)
            {
                await HandleExceptionAsync(context, ex, "Database Confict Error", HttpStatusCode.Conflict);
            }
            catch (DbUpdateException ex)
            {
                await HandleExceptionAsync(context, ex, "Database Update Error", HttpStatusCode.InternalServerError);
            }
            catch (TimeoutException ex)
            {
                await HandleExceptionAsync(context, ex, "Database Connection Timeout Error", HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, "Other Database Connection Error", HttpStatusCode.InternalServerError);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, string message, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "aplication/json";
            context.Response.StatusCode = (int)statusCode;

            var exResponse = new
            {
                statusCode = context.Response.StatusCode,
                message,
                details = ex.Message
            };

            var jsonExResponse = JsonSerializer.Serialize(exResponse);

            await context.Response.WriteAsync(jsonExResponse);
        }
    }
}
