using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MrApi.Data;

namespace MrApi.Tests;

public class ApiTests : IClassFixture<ApiTests.Factory>
{
    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder b)
        {
            b.UseSetting("Api:ApiKey", "secreto");
            b.UseSetting("ConnectionStrings:ExpoMR", "x");
            b.UseSetting("Sucursales:1:Indigo", "x");
            b.UseSetting("Sucursales:2:Indigo", "x");
            b.ConfigureServices(s =>
            {
                s.AddSingleton<IConnections>(new FakeConnections());
                s.AddSingleton<IExpoRepository, FakeExpo>();
                s.AddSingleton<IIndigoRepository, FakeIndigo>();
            });
        }
    }

    private readonly HttpClient _http;
    public ApiTests(Factory f)
    {
        _http = f.CreateClient();
        _http.DefaultRequestHeaders.Add("X-Api-Key", "secreto");
    }

    [Fact]
    public async Task Health_no_requiere_clave()
    {
        var r = await new Factory().CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Sin_clave_o_clave_incorrecta_401()
    {
        var c = new Factory().CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/inventory/1")).StatusCode);
        c.DefaultRequestHeaders.Add("X-Api-Key", "mala");
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/inventory/1")).StatusCode);
    }

    [Fact]
    public async Task Precios_usa_sucursal_del_cliente_y_reporta_sin_precio()
    {
        var r = await _http.PostAsJsonAsync("/prices", new { idPersona = 10, productos = new[] { 1, 2, 2, 3 } });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var body = await r.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal(2, body.GetProperty("sucursal").GetInt32());
        Assert.Equal(2, body.GetProperty("precios").GetArrayLength());
        Assert.Equal(3, body.GetProperty("sinPrecio")[0].GetInt32());
    }

    [Theory]
    [InlineData(0, new[] { 1 })]
    [InlineData(10, new int[0])]
    public async Task Precios_valida_entrada(int persona, int[] productos)
    {
        var r = await _http.PostAsJsonAsync("/prices", new { idPersona = persona, productos });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Precios_limita_tamano_de_lote()
    {
        var r = await _http.PostAsJsonAsync("/prices", new { idPersona = 10, productos = Enumerable.Range(1, 501) });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Precios_cliente_inexistente_404()
    {
        var r = await _http.PostAsJsonAsync("/prices", new { idPersona = 99, productos = new[] { 1 } });
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Credito_combina_indigo_expo_y_back_order()
    {
        var body = await _http.GetFromJsonAsync<System.Text.Json.JsonElement>("/clients/10/credit");
        Assert.Equal(1000m + 200m - 50m, body.GetProperty("disponible").GetDecimal());
    }

    [Fact]
    public async Task Inventario_suma_por_sucursal()
    {
        var body = await _http.GetFromJsonAsync<System.Text.Json.JsonElement>("/inventory/5");
        Assert.True(body.GetProperty("hayExistencia").GetBoolean());
        Assert.Equal(7, body.GetProperty("existencia").GetProperty("qro").GetInt32());
    }

    private sealed class FakeConnections : IConnections
    {
        public IReadOnlyCollection<int> Sucursales => [1, 2];
        public bool EsSucursalValida(int s) => s is 1 or 2;
        public Task<System.Data.Common.DbConnection> ExpoMR(CancellationToken ct) => throw new NotSupportedException();
        public Task<System.Data.Common.DbConnection> Indigo(int s, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeExpo : IExpoRepository
    {
        public Task<int?> SucursalDeCliente(int id, CancellationToken ct) => Task.FromResult<int?>(id == 10 ? 2 : null);
        public Task<(decimal CreditoExpo, decimal MontoBackOrder)> CreditoExpo(int id, CancellationToken ct) => Task.FromResult((200m, 50m));
    }

    private sealed class FakeIndigo : IIndigoRepository
    {
        public Task<IReadOnlyList<PrecioRow>> Precios(int s, int p, IReadOnlyCollection<int> prods, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<PrecioRow>>(prods.Where(x => x != 3).Select(x => new PrecioRow(x, x * 10.5)).ToList());
        public Task<CreditoIndigo?> Credito(int s, int p, CancellationToken ct) => Task.FromResult<CreditoIndigo?>(new(900, 1000));
        public Task<int> ExistenciaNormal(int s, int p, CancellationToken ct) => Task.FromResult(s == 2 ? 7 : 0);
        public Task Ping(int s, CancellationToken ct) => Task.CompletedTask;
    }
}
