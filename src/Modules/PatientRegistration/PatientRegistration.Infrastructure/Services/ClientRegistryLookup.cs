using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Jacana.PatientRegistration.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Jacana.PatientRegistration.Infrastructure.Services;

/// <summary>
/// Client for the Kenya national Client Registry (NUPI) partner API:
///   token:  POST {TokenUrl}   (OAuth2 client_credentials, Basic auth)
///   lookup: GET  {SearchBaseUrl}/{country}/national-id/{nationalId}
/// Tokens are cached in-memory until 60s before expiry (JWT exp claim).
/// All failures (network, auth, 4xx/5xx) are absorbed into RegistryLookupResult —
/// callers must never throw for a lookup.
/// </summary>
public sealed class ClientRegistryLookup : IClientRegistryLookup
{
    private readonly ClientRegistryOptions _options;
    private readonly HttpClient _http;
    private readonly object _tokenLock = new();
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiryUtc = DateTimeOffset.MinValue;

    public ClientRegistryLookup(IOptions<ClientRegistryOptions> options)
    {
        _options = options.Value;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds) };
    }

    public async Task<RegistryLookupResult> LookupByNationalIdAsync(string nationalId, CancellationToken ct = default)
    {
        if (!_options.Enabled)
            return new RegistryLookupResult(false, null, "National registry lookup is disabled.");

        if (string.IsNullOrWhiteSpace(_options.TokenUrl) || string.IsNullOrWhiteSpace(_options.ClientId)
            || string.IsNullOrWhiteSpace(_options.ClientSecret) || string.IsNullOrWhiteSpace(_options.SearchBaseUrl))
        {
            return new RegistryLookupResult(false, null,
                "National registry is not configured on this server (ClientRegistry settings missing).");
        }

        var token = await GetTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
            return new RegistryLookupResult(false, null, "Could not authenticate with the national registry.");

        try
        {
            var url = $"{_options.SearchBaseUrl.TrimEnd('/')}/{_options.CountryCode}/national-id/{Uri.EscapeDataString(nationalId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new RegistryLookupResult(false, null);
            if (!response.IsSuccessStatusCode)
                return new RegistryLookupResult(false, null, $"Registry returned {(int)response.StatusCode}.");

            var body = await response.Content.ReadAsStringAsync(ct);
            return ParseResponse(body);
        }
        catch (OperationCanceledException)
        {
            return new RegistryLookupResult(false, null, "Registry lookup timed out.");
        }
        catch (Exception)
        {
            return new RegistryLookupResult(false, null, "Registry lookup failed (network).");
        }
    }

    private RegistryLookupResult ParseResponse(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var clientExists = root.TryGetProperty("clientExists", out var ce) && ce.ValueKind == JsonValueKind.True;

        if (!clientExists || !root.TryGetProperty("client", out var client) || client.ValueKind != JsonValueKind.Object)
            return new RegistryLookupResult(false, null);

        var clientNumber = GetString(client, "clientNumber");
        if (string.IsNullOrWhiteSpace(clientNumber))
            return new RegistryLookupResult(false, null);

        string? phone = null;
        if (client.TryGetProperty("contact", out var contact) && contact.ValueKind == JsonValueKind.Object)
        {
            var rawPhone = GetString(contact, "primaryPhone");
            if (!string.IsNullOrWhiteSpace(rawPhone))
                phone = NormalizeKenyanPhone(rawPhone);
        }

        string? county = null, subCounty = null, ward = null, village = null;
        if (client.TryGetProperty("residence", out var residence) && residence.ValueKind == JsonValueKind.Object)
        {
            county = GetString(residence, "county");
            subCounty = GetString(residence, "subCounty");
            ward = GetString(residence, "ward");
            village = GetString(residence, "village");
        }

        var dobRaw = GetString(client, "dateOfBirth");
        DateOnly? dob = null;
        if (!string.IsNullOrWhiteSpace(dobRaw) && DateOnly.TryParse(dobRaw, out var parsed))
            dob = parsed;

        var gender = GetString(client, "gender")?.ToLowerInvariant() switch
        {
            "male" => "Male",
            "female" => "Female",
            _ => null
        };

        var dto = new RegistryClientDto(
            clientNumber.Trim(),
            GetString(client, "firstName"),
            GetString(client, "middleName"),
            GetString(client, "lastName"),
            dob,
            gender,
            phone,
            county,
            subCounty,
            ward,
            village);

        return new RegistryLookupResult(true, dto);
    }

    private async Task<string?> GetTokenAsync(CancellationToken ct)
    {
        lock (_tokenLock)
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiryUtc)
                return _cachedToken;
        }

        try
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = _options.Scope,
            });
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var token = root.TryGetProperty("access_token", out var t) ? t.GetString() : null;
            if (string.IsNullOrWhiteSpace(token)) return null;

            var expiresIn = root.TryGetProperty("expires_in", out var e) && e.TryGetInt64(out var secs) ? secs : 3600;
            lock (_tokenLock)
            {
                _cachedToken = token;
                _tokenExpiryUtc = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 60);
            }
            return token;
        }
        catch
        {
            return null;
        }
    }

    private static string? GetString(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static string? NormalizeKenyanPhone(string raw)
    {
        // Registry may return "+2547…", "2547…" or "07…" — canonicalise to E.164.
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length == 12 && digits.StartsWith("254")) return "+" + digits;
        if (digits.Length == 10 && digits.StartsWith("0")) return "+254" + digits[1..];
        if (digits.Length == 9 && (digits[0] == '7' || digits[0] == '1')) return "+254" + digits;
        return null;
    }
}
