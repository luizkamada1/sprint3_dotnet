using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MottuYardApi.Security
{
    public class ApiKeyEndpointFilter : IEndpointFilter
    {
        private readonly string _expectedApiKey;

        public ApiKeyEndpointFilter(IOptions<ApiKeyOptions> options)
        {
            _expectedApiKey = options.Value.ApiKey ?? string.Empty;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (string.IsNullOrWhiteSpace(_expectedApiKey))
            {
                return Results.Problem(
                    "API Key não configurada.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyConstants.HeaderName, out var header) ||
                !string.Equals(header.ToString(), _expectedApiKey, StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            return await next(context);
        }
    }
}
