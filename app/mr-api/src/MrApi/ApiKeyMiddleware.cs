using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace MrApi;

/// <summary>Exige X-Api-Key en todo salvo /health. Sin clave configurada, rechaza todo (falla cerrado).</summary>
public sealed class ApiKeyMiddleware(RequestDelegate next, IOptions<ApiOptions> options)
{
    private readonly byte[] _key = SHA256.HashData(Encoding.UTF8.GetBytes(options.Value.ApiKey));
    private readonly bool _configurada = !string.IsNullOrEmpty(options.Value.ApiKey);

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.Request.Path.StartsWithSegments("/health")) { await next(ctx); return; }

        var enviada = ctx.Request.Headers["X-Api-Key"].ToString();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(enviada));
        if (!_configurada || !CryptographicOperations.FixedTimeEquals(hash, _key))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        await next(ctx);
    }
}
