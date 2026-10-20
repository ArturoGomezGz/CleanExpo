using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MrApi.Data;

/// <summary>Única fuente de conexiones: ExpoMR y un Indigo por sucursal (1 GDL, 2 QRO, 3 MEX).</summary>
public interface IConnections
{
    IReadOnlyCollection<int> Sucursales { get; }
    bool EsSucursalValida(int sucursal);
    Task<DbConnection> ExpoMR(CancellationToken ct);
    Task<DbConnection> Indigo(int sucursal, CancellationToken ct);
}

public sealed class SqlConnections : IConnections
{
    private readonly string _expoMr;
    private readonly Dictionary<int, string> _indigo;

    public SqlConnections(IConfiguration config)
    {
        _expoMr = config.GetConnectionString("ExpoMR") ?? "";
        _indigo = config.GetSection("Sucursales").GetChildren()
            .Where(s => int.TryParse(s.Key, out _))
            .ToDictionary(s => int.Parse(s.Key), s => s["Indigo"] ?? "");
    }

    public IReadOnlyCollection<int> Sucursales => _indigo.Keys;
    public bool EsSucursalValida(int sucursal) => _indigo.ContainsKey(sucursal);

    public async Task<DbConnection> ExpoMR(CancellationToken ct) => await Open(_expoMr, ct);

    public async Task<DbConnection> Indigo(int sucursal, CancellationToken ct) =>
        await Open(_indigo[sucursal], ct);

    private static async Task<DbConnection> Open(string cs, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("Cadena de conexión no configurada.");
        var con = new SqlConnection(cs);
        try { await con.OpenAsync(ct); return con; }
        catch { await con.DisposeAsync(); throw; }
    }
}
