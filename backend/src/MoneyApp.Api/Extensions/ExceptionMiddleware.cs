using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyApp.Application.Common;

namespace MoneyApp.Api.Extensions;

public partial class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
{
    public async Task Invoke(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (AppException e) { await Write(ctx, e.Status, e.Code, e.Message); }
        catch (OperationCanceledException) { ctx.Response.StatusCode = 499; }
        catch (Exception e)
        {   // never leak internals to clients
            LogUnhandledException(log, e, ctx.TraceIdentifier);
            await Write(ctx, 500, "server_error", "An unexpected error occurred.");
        }
    }

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Unhandled exception {TraceId}")]
    private static partial void LogUnhandledException(
        ILogger logger,
        Exception exception,
        string traceId);

    private static Task Write(HttpContext ctx, int status, string code, string message)
    {
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = code, Detail = message });
    }
}
