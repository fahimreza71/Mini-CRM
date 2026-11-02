using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniCrm.Application.Common;
using MiniCrm.Domain.Common;

namespace MiniCrm.Api.Errors;

public sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            DomainException e => new ProblemDetails
            { Status = StatusCodes.Status400BadRequest, Title = "A business rule was violated.", Detail = e.Message },
            NotFoundException e => new ProblemDetails
            { Status = StatusCodes.Status404NotFound, Title = "Resource not found.", Detail = e.Message },
            ConflictException e => new ProblemDetails
            { Status = StatusCodes.Status409Conflict, Title = "Conflict.", Detail = e.Message },
            _ => null
        };

        if (problem is null) return false;   // not ours: the default handler returns a safe 500

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}