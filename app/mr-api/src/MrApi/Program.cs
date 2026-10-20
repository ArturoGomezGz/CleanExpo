using Microsoft.Data.SqlClient;
using MrApi;
using MrApi.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<IConnections, SqlConnections>();
builder.Services.AddSingleton<IExpoRepository, ExpoRepository>();
builder.Services.AddSingleton<IIndigoRepository, IndigoRepository>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// SQL Server caído o sin permisos -> 503 para que el worker reintente (no 500).
app.UseExceptionHandler(h => h.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var transitorio = ex is SqlException or InvalidOperationException { Message: "Cadena de conexión no configurada." };
    ctx.Response.StatusCode = transitorio ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status500InternalServerError;
    if (ex is not null)
        ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("MrApi").LogError(ex, "Error en {Path}", ctx.Request.Path);
    await ctx.Response.WriteAsJsonAsync(new { error = transitorio ? "sql_no_disponible" : "error_interno" });
}));
app.UseMiddleware<ApiKeyMiddleware>();

app.MapEndpoints();
app.Run();

public partial class Program;
