using SportMarket.Application.Common.Exceptions;

namespace SportMarket.Api.Middleware;

/// <summary>تبدیل استثناها به پاسخ‌های HTTP با ساختار یکسان</summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, message, errors) = exception switch
        {
            ValidationException validation => (StatusCodes.Status400BadRequest, validation.Message, (object?)validation.Errors),
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message, null),
            UnauthorizedException unauthorized => (StatusCodes.Status401Unauthorized, unauthorized.Message, null),
            ConflictException conflict => (StatusCodes.Status409Conflict, conflict.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "خطای غیرمنتظره‌ای در سرور رخ داد.", null)
        };

        if (status >= 500)
        {
            _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Handled error {Status} on {Path}: {Message}", status, context.Request.Path, message);
        }

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";

        object payload = errors is not null
            ? new { message, errors }
            : new { message, errors = (IDictionary<string, string[]>?)null };

        await context.Response.WriteAsJsonAsync(payload);
    }
}
