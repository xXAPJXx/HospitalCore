using HospitalCore.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace HospitalCore.Api.Errors;

internal static class ErrorResults
{
    public static ObjectResult ToProblem(this ControllerBase controller, Error error)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };

        var problem = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode,
            title,
            detail: error.Description);
        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" },
        };
    }
}
