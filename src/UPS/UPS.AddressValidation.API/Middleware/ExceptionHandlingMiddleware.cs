using System.Text.Json;
using FluentValidation;
using UPS.AddressValidation.Application.Exceptions;

namespace UPS.AddressValidation.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await WriteValidationErrorAsync(context, exception);
        }
        catch (ExternalApiException exception)
        {
            logger.LogWarning(
                exception,
                "UPS external API request failed with status {StatusCode}.",
                exception.StatusCode);

            await WriteExternalApiErrorAsync(
                context,
                exception);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "HTTP communication with UPS failed.");

            await WriteJsonAsync(
                context,
                StatusCodes.Status502BadGateway,
                new
                {
                    success = false,
                    message = "Unable to communicate with the UPS service.",
                    errors = Array.Empty<object>()
                });
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception.");

            await WriteJsonAsync(
                context,
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    errors = Array.Empty<object>()
                });
        }
    }

    private static Task WriteValidationErrorAsync(
        HttpContext context,
        ValidationException exception)
    {
        var errors =
            exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(x => x.ErrorMessage)
                        .Distinct()
                        .ToArray());

        return WriteJsonAsync(
            context,
            StatusCodes.Status400BadRequest,
            new
            {
                success = false,
                message = "Validation failed.",
                errors
            });
    }

    private static Task WriteExternalApiErrorAsync(
        HttpContext context,
        ExternalApiException exception)
    {
        var errors =
            exception.Errors
                .Select(error => new
                {
                    code = error.Code,
                    message = error.Message
                })
                .ToArray();

        return WriteJsonAsync(
            context,
            exception.StatusCode is >= 400 and <= 599
                ? exception.StatusCode
                : StatusCodes.Status502BadGateway,
            new
            {
                success = false,
                message = exception.Message,
                errors
            });
    }

    private static async Task WriteJsonAsync(
        HttpContext context,
        int statusCode,
        object body)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, JsonOptions));
    }
}
