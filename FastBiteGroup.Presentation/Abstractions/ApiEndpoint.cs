using FastBiteGroup.Contract.Abstractions.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FastBiteGroup.Presentation.Abstractions
{
    public abstract class ApiEndpoint
    {
        protected static IResult HandleFailure(Result result) =>
        result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException("Cannot handle failure for a successful result."),
            IValidationResult validationResult =>
                Results.BadRequest(
                    CreateProblemDetails("Validation Error", StatusCodes.Status400BadRequest, result.Error, validationResult.Errors)),
            _ => Results.Problem(CreateProblemDetails(
                    GetTitle(result.Error),
                    GetStatusCode(result.Error),
                    result.Error))
        };

        protected static IResult HandleFailure<TValue>(Result<TValue> results) =>
            results switch
            {
                { IsSuccess: true } => throw new InvalidOperationException(),
                IValidationResult validationResult => Results.BadRequest(CreateProblemDetails("Validation Error", StatusCodes.Status400BadRequest, results.Error, validationResult.Errors)),
                _ => Results.BadRequest(CreateProblemDetails("Bad Request", StatusCodes.Status400BadRequest, results.Error))
            };

        // Tạo các hàm private để ánh xạ (Mapping) mã lỗi sang HTTP Status Code
        private static int GetStatusCode(Error error)
        {
            if (error.Code.Contains("NotFound", StringComparison.InvariantCultureIgnoreCase))
                return StatusCodes.Status404NotFound;

            if (error.Code.Contains("Conflict", StringComparison.InvariantCultureIgnoreCase))
                return StatusCodes.Status409Conflict;

            if (error.Code.Contains("Unauthorized", StringComparison.InvariantCultureIgnoreCase))
                return StatusCodes.Status401Unauthorized;

            if (error.Code.Contains("Forbidden", StringComparison.InvariantCultureIgnoreCase))
                return StatusCodes.Status403Forbidden;

            return StatusCodes.Status400BadRequest; // Default
        }

        private static string GetTitle(Error error) =>
            GetStatusCode(error) switch
            {
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                _ => "Bad Request"
            };


        private static ProblemDetails CreateProblemDetails(string title, int status, Error error, Error[]? errors = null)
        {
            var problemDetails = new ProblemDetails
            {
                Title = title,
                Type = error.Code,
                Detail = error.Message,
                Status = status
            };

            if (errors is not null)
            {
                problemDetails.Extensions.Add("errors", errors);
            }

            return problemDetails;
        }
    }
}
