using System.Text.Json;
using FluentValidation; 
using UPS.AddressSuggestion.Infrastructure.Exceptions;

namespace UPS.AddressSuggestion.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteAsync(
                context,
                ex.Errors.Select(e => new
                {
                    code = e.PropertyName,
                    message = e.ErrorMessage
                }));
        }
        catch (UpsApiException ex)
        {
            logger.LogWarning(
                "UPS API returned status {StatusCode}, error code {UpsErrorCode}.",
                ex.StatusCode,
                ex.UpsErrorCode);

            context.Response.StatusCode =
                ex.StatusCode is >= 400 and <= 499
                    ? ex.StatusCode
                    : StatusCodes.Status502BadGateway;

            await WriteAsync(
                context,
                new[]
                {
                    new
                    {
                        code = ex.UpsErrorCode ?? "UPS_API_ERROR",
                        message = ex.Message
                    }
                });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error while calling UPS.");
            context.Response.StatusCode = StatusCodes.Status502BadGateway;

            await WriteAsync(
                context,
                new[]
                {
                    new
                    {
                        code = "UPS_NETWORK_ERROR",
                        message = "Unable to communicate with UPS."
                    }
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await WriteAsync(
                context,
                new[]
                {
                    new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        message = "An unexpected error occurred."
                    }
                });
        }
    }

    private static async Task WriteAsync(
        HttpContext context,
        object errors)
    {
        context.Response.ContentType = "application/json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            new
            {
                success = false,
                errors,
                timestamp = DateTimeOffset.UtcNow
            });
    }
}