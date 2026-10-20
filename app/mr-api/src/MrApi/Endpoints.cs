using MrApi.Data;

namespace MrApi;

public sealed record PreciosRequest(int IdPersona, int[]? Productos);

public static class Endpoints
{
    public const int MaxProductosPorConsulta = 500;

    public static void MapEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        // Readiness: verifica ExpoMR y cada Indigo. 503 si alguno falla.
        app.MapGet("/health/ready", async (IConnections cn, IIndigoRepository indigo, IExpoRepository expo, CancellationToken ct) =>
        {
            var estado = new Dictionary<string, string>();
            try { await expo.SucursalDeCliente(0, ct); estado["ExpoMR"] = "ok"; } catch { estado["ExpoMR"] = "error"; }
            foreach (var s in cn.Sucursales)
            {
                try { await indigo.Ping(s, ct); estado[$"Indigo{s}"] = "ok"; } catch { estado[$"Indigo{s}"] = "error"; }
            }
            return estado.Values.All(v => v == "ok")
                ? Results.Ok(estado)
                : Results.Json(estado, statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        // Precios por cliente: la lógica vive en Indigo.GetMultiPrecioExpo, no se reimplementa.
        app.MapPost("/prices", async (PreciosRequest req, IConnections cn, IExpoRepository expo, IIndigoRepository indigo, CancellationToken ct) =>
        {
            if (req.IdPersona <= 0) return Results.BadRequest(new { error = "idPersona inválido" });
            var productos = (req.Productos ?? []).Where(p => p > 0).Distinct().ToArray();
            if (productos.Length == 0) return Results.BadRequest(new { error = "productos requerido" });
            if (productos.Length > MaxProductosPorConsulta)
                return Results.BadRequest(new { error = $"máximo {MaxProductosPorConsulta} productos por consulta" });

            var sucursal = await expo.SucursalDeCliente(req.IdPersona, ct);
            if (sucursal is null) return Results.NotFound(new { error = "cliente no existe" });
            if (!cn.EsSucursalValida(sucursal.Value)) return Results.UnprocessableEntity(new { error = "sucursal del cliente no configurada" });

            var precios = await indigo.Precios(sucursal.Value, req.IdPersona, productos, ct);
            var conPrecio = precios.Select(p => p.IdProducto).ToHashSet();
            return Results.Ok(new
            {
                idPersona = req.IdPersona,
                sucursal,
                precios = precios.Select(p => new { idProducto = p.IdProducto, precio = p.Precio }),
                sinPrecio = productos.Where(p => !conPrecio.Contains(p))
            });
        });

        // Crédito disponible = crédito Indigo de la sucursal + mClientes.Credito (Expo) - back order (ExpoMR.GetCredit).
        app.MapGet("/clients/{idPersona:int}/credit", async (int idPersona, IConnections cn, IExpoRepository expo, IIndigoRepository indigo, CancellationToken ct) =>
        {
            var sucursal = await expo.SucursalDeCliente(idPersona, ct);
            if (sucursal is null) return Results.NotFound(new { error = "cliente no existe" });
            if (!cn.EsSucursalValida(sucursal.Value)) return Results.UnprocessableEntity(new { error = "sucursal del cliente no configurada" });

            var indigoCredito = await indigo.Credito(sucursal.Value, idPersona, ct);
            if (indigoCredito is null) return Results.NotFound(new { error = "cliente sin crédito en Indigo" });
            var (creditoExpo, backOrder) = await expo.CreditoExpo(idPersona, ct);

            return Results.Ok(new
            {
                idPersona,
                sucursal,
                credito = indigoCredito.Credito,
                creditoNeto = indigoCredito.CreditoNeto,
                creditoInyectado = indigoCredito.CreditoInyectado,
                creditoExpo,
                montoBackOrder = backOrder,
                disponible = indigoCredito.Credito + creditoExpo - backOrder
            });
        });

        // Existencia normal (IdCondicion=1) por sucursal; solo se usa para productos en liquidación.
        app.MapGet("/inventory/{idProducto:int}", async (int idProducto, IConnections cn, IIndigoRepository indigo, CancellationToken ct) =>
        {
            if (idProducto <= 0) return Results.BadRequest(new { error = "idProducto inválido" });
            var salida = new Dictionary<string, int>();
            foreach (var s in cn.Sucursales.Order())
                salida[s switch { 1 => "gdl", 2 => "qro", 3 => "mex", _ => $"s{s}" }] = await indigo.ExistenciaNormal(s, idProducto, ct);
            return Results.Ok(new { idProducto, existencia = salida, hayExistencia = salida.Values.Sum() > 0 });
        });
    }
}
