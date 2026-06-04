using FastBiteGroup.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FastBiteGroup.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var (statusCode, title) = exception switch
            {
                BadRequestException badRequestException => (StatusCodes.Status400BadRequest, badRequestException.Title),
                NotFoundException notFoundException => (StatusCodes.Status404NotFound, notFoundException.Title),
                FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = _environment.IsDevelopment() || statusCode < 500
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                Instance = httpContext.Request.Path
            };

            if (exception is FluentValidation.ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Errors
                    .Select(e => new { e.PropertyName, e.ErrorMessage })
                    .ToArray();
            }

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
