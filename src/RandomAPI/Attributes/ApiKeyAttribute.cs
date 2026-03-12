using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// RAttribute forcing a requirement for an API key. can only be placed on a controller class by placing the [ApiKey] attribute on top of it
/// </summary>
public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "X-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var secretKey = config.GetValue<string>("Authentication:ApiKey");

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey) ||
            secretKey != providedKey)
        {
            context.Result = new UnauthorizedObjectResult("Invalid or missing API Key.");
            return;
        }

        await next();
    }
}
