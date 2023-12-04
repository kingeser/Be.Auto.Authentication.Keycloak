using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Be.Auto.Authentication.Keycloak.Role;

public class KeycloakAsyncAuthorizationFilter : IAsyncAuthorizationFilter
{

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .Any())
        {
            return Task.CompletedTask;
        }

        switch (context.ActionDescriptor)
        {
            case ControllerActionDescriptor controllerActionDescriptor:
                {
                    var controllerName = RoleExtension.Normalize(controllerActionDescriptor.ControllerTypeInfo.Name);
                    var actionName = controllerActionDescriptor.ActionName;
                    var controllerRole = $"Controllers.{controllerName}.{actionName}";

                    if (!context.HttpContext.User.IsInRole(controllerRole))
                    {
                        context.Result = new UnauthorizedResult();
                    }

                    break;
                }
            case CompiledPageActionDescriptor pageActionDescriptor:
                {
                    var pageRole = $"Pages.{RoleExtension.Normalize(pageActionDescriptor.ModelTypeInfo?.Name ?? "")}";

                    if (!context.HttpContext.User.IsInRole(pageRole))
                    {
                        context.Result = new UnauthorizedResult();
                    }
                    break;
                }
        }

        return Task.CompletedTask;
    }
}