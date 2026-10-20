using Dapper;

namespace MrApi.Data;

public sealed record PrecioRow(int IdProducto, double Precio);
public sealed record CreditoIndigo(decimal Credito, decimal CreditoNeto, decimal CreditoInyectado);
public sealed record InventarioSucursal(int Sucursal, int Cantidad);

public interface IExpoRepository
{
    /// <summary>mClientes.Sucursal del cliente, o null si no existe.</summary>
    Task<int?> SucursalDeCliente(int idPersona, CancellationToken ct);
    /// <summary>mClientes.Credito (crédito propio de la Expo) y GetMontoBackOrder.</summary>
    Task<(decimal CreditoExpo, decimal MontoBackOrder)> CreditoExpo(int idPersona, CancellationToken ct);
}

public interface IIndigoRepository
{
    Task<IReadOnlyList<PrecioRow>> Precios(int sucursal, int idPersona, IReadOnlyCollection<int> productos, CancellationToken ct);
    Task<CreditoIndigo?> Credito(int sucursal, int idPersona, CancellationToken ct);
    Task<int> ExistenciaNormal(int sucursal, int idProducto, CancellationToken ct);
    Task Ping(int sucursal, CancellationToken ct);
}

public sealed class ExpoRepository(IConnections cn) : IExpoRepository
{
    public async Task<int?> SucursalDeCliente(int idPersona, CancellationToken ct)
    {
        await using var con = await cn.ExpoMR(ct);
        return await con.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
            "SELECT Sucursal FROM mClientes WHERE IdPersona = @idPersona", new { idPersona }, cancellationToken: ct));
    }

    public async Task<(decimal, decimal)> CreditoExpo(int idPersona, CancellationToken ct)
    {
        await using var con = await cn.ExpoMR(ct);
        var row = await con.QuerySingleAsync<(decimal, decimal)>(new CommandDefinition(
            """
            SELECT ISNULL((SELECT Credito FROM mClientes WHERE IdPersona = @idPersona), 0),
                   ISNULL(dbo.GetMontoBackOrder(@idPersona), 0)
            """, new { idPersona }, cancellationToken: ct));
        return row;
    }
}

public sealed class IndigoRepository(IConnections cn) : IIndigoRepository
{
    private const int LoteMax = 1000; // límite de filas de INSERT ... VALUES

    public async Task<IReadOnlyList<PrecioRow>> Precios(int sucursal, int idPersona, IReadOnlyCollection<int> productos, CancellationToken ct)
    {
        await using var con = await cn.Indigo(sucursal, ct);
        var salida = new List<PrecioRow>(productos.Count);
        foreach (var lote in productos.Chunk(LoteMax))
        {
            // Los ids son int validados: se formatean en el SQL para no depender del esquema exacto de TTCotizar.
            var values = string.Join(",", lote.Select(p => $"({idPersona},{p})"));
            var sql = $"""
                DECLARE @datos TTCotizar;
                INSERT @datos (IdPersona, IdProducto) VALUES {values};
                SELECT IdProducto, CAST(Precio AS float) AS Precio FROM dbo.GetMultiPrecioExpo(@datos);
                """;
            var rows = await con.QueryAsync<PrecioRow>(new CommandDefinition(sql, cancellationToken: ct));
            salida.AddRange(rows);
        }
        return salida;
    }

    public async Task<CreditoIndigo?> Credito(int sucursal, int idPersona, CancellationToken ct)
    {
        await using var con = await cn.Indigo(sucursal, ct);
        return await con.QuerySingleOrDefaultAsync<CreditoIndigo>(new CommandDefinition(
            "SELECT Credito, CreditoNeto, CreditoInyectado FROM vCredito WHERE IdPersona = @idPersona",
            new { idPersona }, cancellationToken: ct));
    }

    public async Task<int> ExistenciaNormal(int sucursal, int idProducto, CancellationToken ct)
    {
        await using var con = await cn.Indigo(sucursal, ct);
        // IdCondicion 1 = Normal (misma condición que ExpoMR.GetInvSucursales)
        return await con.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT ISNULL(SUM(Cantidad), 0) FROM mProdVenta WHERE IdProducto = @idProducto AND IdCondicion = 1",
            new { idProducto }, cancellationToken: ct));
    }

    public async Task Ping(int sucursal, CancellationToken ct)
    {
        await using var con = await cn.Indigo(sucursal, ct);
        await con.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1", cancellationToken: ct));
    }
}
