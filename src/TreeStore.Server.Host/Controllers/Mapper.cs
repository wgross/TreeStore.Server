using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.Server.Host.Controllers
{
    public static class Mapper
    {
        public static EntityResponse ToDto(this Entity entity)
        {
            return new EntityResponse(entity.Id, entity.Name);
        }

        public static async Task<IActionResult> MapExceptions<C>(this C controller, Func<Task<IActionResult>> action)
            where C : ControllerBase
        {
            try
            {
                return await action();
            }
            catch (ArgumentNullException ex)
            {
                var details = new ProblemDetails
                {
                    Detail = ex.Message,
                };
                details.Extensions["ErrorType"] = nameof(ArgumentNullException);
                details.Extensions[nameof(ArgumentNullException.ParamName)] = ex.ParamName;

                return controller.BadRequest(details);
            }
            catch (Exception ex)
            {
                var details = new ProblemDetails
                {
                    Detail = ex.Message,
                };
                details.Extensions["ErrorType"] = ex.GetType().Name;
                return controller.BadRequest(details);
            }
        }

        public static IActionResult MapExceptions<C>(this C controller, Func<IActionResult> action)
            where C : ControllerBase
        {
            try
            {
                return action();
            }
            catch (ArgumentNullException ex)
            {
                var details = new ProblemDetails
                {
                    Detail = ex.Message,
                };
                details.Extensions["ErrorType"] = nameof(ArgumentNullException);
                details.Extensions[nameof(ArgumentNullException.ParamName)] = ex.ParamName;

                return controller.BadRequest(details);
            }
            catch (Exception ex)
            {
                var details = new ProblemDetails
                {
                    Detail = ex.Message,
                };
                details.Extensions["ErrorType"] = ex.GetType().Name;
                return controller.BadRequest(details);
            }
        }
    }
}