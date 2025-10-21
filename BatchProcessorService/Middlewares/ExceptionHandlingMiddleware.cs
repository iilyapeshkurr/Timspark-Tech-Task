using System.Net;
using System.Text.Json;
using BatchProcessorService.Exceptions;

namespace BatchProcessorService.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> _logger)
{
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode statusCode;
        string message;

        switch (ex)
        {
            case BadRequestException:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = ex.Message;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = ex.Message;
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
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
