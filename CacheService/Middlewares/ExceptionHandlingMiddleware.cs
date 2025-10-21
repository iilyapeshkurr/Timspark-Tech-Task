using System.Net;
using System.Text.Json;
using CacheService.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CacheService.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        string message = "An unexpected error occurred.";

        switch (ex)
        {
            case BadRequestException:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = ex.Message;
                break;
        }

        var errorResponse = new
        {
            status = (int)statusCode,
            message,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
