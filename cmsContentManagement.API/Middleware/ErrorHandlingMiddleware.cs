using System.Net;
using System.Text.Json;
using cmsContentManagement.Application.Common.ErrorCodes;

namespace cmsContentManagement.Middleware;

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
        catch (GeneralErrorCodes ex)
        {
            await HandleCustomErrorAsync(context, ex);
        }
        catch (AuthErrorCodes ex)
        {
            await HandleCustomAuthErrorAsync(context, ex, HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            await HandleGenericErrorAsync(context, ex);
        }
    }

    private static Task HandleCustomErrorAsync(
        HttpContext context,
        GeneralErrorCodes error)
    {
        context.Response.ContentType = "application/json";
        
        HttpStatusCode status = error.Code switch
        {
            1 => HttpStatusCode.NotFound,
            2 => HttpStatusCode.Conflict,
            3 => HttpStatusCode.BadRequest,
            4 => HttpStatusCode.InternalServerError,
            5 => HttpStatusCode.InternalServerError,
            6 => HttpStatusCode.Conflict,
            7 => HttpStatusCode.ServiceUnavailable,
            8 => HttpStatusCode.Forbidden,
            9 => HttpStatusCode.BadRequest,
            11 => HttpStatusCode.BadRequest,
            12 => HttpStatusCode.BadRequest,
            13 => HttpStatusCode.BadRequest,
            14 => HttpStatusCode.Conflict,
            15 => HttpStatusCode.BadRequest,
            16 => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int) status;

        string result = JsonSerializer.Serialize(new { error.Code, error.Message });

        return context.Response.WriteAsync(result);
    }

    private static Task HandleCustomAuthErrorAsync(
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
