using System.Net;
using System.Text.Json;
using cmsContentManagment.Application.Common.ErrorCodes;

namespace cmsContentManagment.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AuthErrorCodes ex)
        {
            await HandleCustomErrorAsync(context, ex, HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            await HandleGenericErrorAsync(context, ex);
        }
    }

    private static Task HandleCustomErrorAsync(
        HttpContext context,
        AuthErrorCodes error,
        HttpStatusCode status)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) status;

        string result = JsonSerializer.Serialize(new { error.Code, error.Message });

        return context.Response.WriteAsync(result);
    }

    private static Task HandleGenericErrorAsync(HttpContext context, Exception error)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

        string result = JsonSerializer.Serialize(new { Code = -1, error.Message });

        return context.Response.WriteAsync(result);
    }
}
