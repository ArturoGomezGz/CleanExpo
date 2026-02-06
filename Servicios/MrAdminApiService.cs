using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ExpoMR.Servicios;

public sealed class MrAdminApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MrAdminApiService> _logger;

    public MrAdminApiService(IHttpClientFactory httpClientFactory, ILogger<MrAdminApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(string baseUrl, string userName, string password, CancellationToken ct = default)
    {
        using var http = _httpClientFactory.CreateClient();
        http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");

        // La API anterior usa x-www-form-urlencoded:
        // username, password, grant_type=password
        var form = new Dictionary<string, string>
        {
            ["username"] = userName,
            ["password"] = password,
            ["grant_type"] = "password"
        };

        var safeLog = $"username={userName}&grant_type=password";
        _logger.LogInformation("[MrAdminApi] POST {Url} form={Form}", new Uri(http.BaseAddress!, "token"), safeLog);

        using var content = new FormUrlEncodedContent(form);
        using var resp = await http.PostAsync("token", content, ct);

        var body = await resp.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("[MrAdminApi] <- {StatusCode} {ReasonPhrase} body={Body}", (int)resp.StatusCode, resp.ReasonPhrase, Truncate(body, 1500));

        resp.EnsureSuccessStatusCode();

        // Respuesta esperada:
        // { access_token: "...", token_type: "bearer" }
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.TryGetProperty("access_token", out var at))
            return at.GetString() ?? string.Empty;
        if (root.TryGetProperty("accessToken", out var at2))
            return at2.GetString() ?? string.Empty;
        if (root.TryGetProperty("token", out var t))
            return t.GetString() ?? string.Empty;

        throw new InvalidOperationException("No fue posible leer el token de la respuesta de /token.");
    }

    public async Task<HttpResponseMessage> CreateUserAsync(string baseUrl, string token, string correo, string contrasena, int rol, CancellationToken ct = default)
    {
        using var http = CreateAuthorizedClient(baseUrl, token);
        var payload = new { correo, contrasena, rol };
        return await SendLoggedAsync(http, HttpMethod.Post, "admin/usuario/nuevo", payload, ct);
    }

    public async Task<HttpResponseMessage> ChangePasswordAsync(string baseUrl, string token, string correo, string nuevaContrasena, CancellationToken ct = default)
    {
        using var http = CreateAuthorizedClient(baseUrl, token);
        var payload = new { correo, nuevaContrasena };
        return await SendLoggedAsync(http, HttpMethod.Put, "admin/usuario/cambiar-contrasena", payload, ct);
    }

    public async Task<HttpResponseMessage> ChangeStatusAsync(string baseUrl, string token, string correo, int status, CancellationToken ct = default)
    {
        using var http = CreateAuthorizedClient(baseUrl, token);
        var payload = new { correo, status };
        return await SendLoggedAsync(http, HttpMethod.Put, "admin/usuario/cambiar-status", payload, ct);
    }

    public async Task<HttpResponseMessage> ChangeRoleAsync(string baseUrl, string token, string correo, int newRol, CancellationToken ct = default)
    {
        using var http = CreateAuthorizedClient(baseUrl, token);
        var payload = new { correo, newRol };
        return await SendLoggedAsync(http, HttpMethod.Put, "admin/usuario/cambiar-rol", payload, ct);
    }

    public async Task<HttpResponseMessage> DeleteUserAsync(string baseUrl, string token, string correo, CancellationToken ct = default)
    {
        using var http = CreateAuthorizedClient(baseUrl, token);
        var payload = new { correo };
        return await SendLoggedAsync(http, HttpMethod.Delete, "admin/usuario/eliminar", payload, ct);
    }

    private async Task<HttpResponseMessage> SendLoggedAsync(HttpClient http, HttpMethod method, string relativeUrl, object payload, CancellationToken ct)
    {
        var url = new Uri(http.BaseAddress!, relativeUrl);
        var json = JsonSerializer.Serialize(payload);

        _logger.LogInformation("[MrAdminApi] {Method} {Url} payload={Payload}", method.Method, url, Truncate(json, 1500));

        using var req = new HttpRequestMessage(method, relativeUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var resp = await http.SendAsync(req, ct);

        var body = await resp.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("[MrAdminApi] <- {StatusCode} {ReasonPhrase} body={Body}", (int)resp.StatusCode, resp.ReasonPhrase, Truncate(body, 2000));

        return resp;
    }

    private static HttpClient CreateAuthorizedClient(string baseUrl, string token)
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return http;
    }

    private static string Truncate(string? value, int max)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= max ? value : value.Substring(0, max) + "...";
    }
}
