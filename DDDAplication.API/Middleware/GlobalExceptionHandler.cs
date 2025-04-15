using DDDAplication.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DDDAplication.API.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path,
                Title = "An unexpected error occurred.",
                Status = (int)HttpStatusCode.InternalServerError
            };

            if (exception is BaseException bex)
            {
                problemDetails.Title = bex.Message;
                problemDetails.Status = (int)bex.StatusCode;
            }
            else
            {
                problemDetails.Detail = exception.Message;
            }

            logger.LogError(exception, "[Exception] {Title}", problemDetails.Title);

            httpContext.Response.ContentType = "application/problem+json";
            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}