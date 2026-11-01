using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Nutrihabitocc.Api.Dtos;

namespace Nutrihabitocc.Api.Services;

public class ApiAuthorizationMiddlewareResultHandler: IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Challenged && !authorizeResult.Forbidden)
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = authorizeResult.Challenged
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status403Forbidden;

        context.Response.ContentType = "application/json";

        var message = authorizeResult.Challenged
            ? "Debes iniciar sesión para acceder a este recurso."
            : "No tienes permisos para realizar esta acción.";

        await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(message));
    }
}
