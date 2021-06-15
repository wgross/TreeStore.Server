using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System;
using System.Net;
using System.Threading.Tasks;

namespace TreeStore.Server.Host.Middleware
{
    public class MapExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ProblemDetailsFactory problemDetailsFactory;
        private readonly IActionResultExecutor<JsonResult> executor;

        public MapExceptionMiddleware(RequestDelegate request, ProblemDetailsFactory problemDetailsFactory, IActionResultExecutor<JsonResult> executor)
        {
            this.next = request;
            this.problemDetailsFactory = problemDetailsFactory;
            this.executor = executor;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await this.next(httpContext);
            }
            catch (Exception ex)
            {
                var problemDetails = this.problemDetailsFactory.CreateProblemDetails(
                    httpContext,
                    statusCode: (int)HttpStatusCode.BadRequest,
                    detail: ex.InnerException is null ? ex.Message : $"{ex.Message}: {ex.InnerException.Message}");

                problemDetails.Extensions["ErrorType"] = ex.GetType().Name;

                if (ex is ArgumentNullException aex)
                {
                    problemDetails.Extensions[nameof(ArgumentNullException.ParamName)] = aex.ParamName;
                }
                await this.WriteDetails(httpContext, problemDetails);
            }
        }

        private async Task WriteDetails(HttpContext httpContext, ProblemDetails problemDetails)
        {
            var routeData = httpContext.GetRouteData();

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor: new());

            var result = new JsonResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
            };

            await this.executor.ExecuteAsync(actionContext, result);

            await httpContext.Response.CompleteAsync();
        }
    }
}