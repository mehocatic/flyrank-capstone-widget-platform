using System.Globalization;
using System.Net;
using System.Text.Json;
using Flyrank.WidgetPlatform.Api.Domain.Models;

namespace Flyrank.WidgetPlatform.Api.Services;

public sealed class GeoEnrichmentService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<GeoEnrichmentService> logger) : IGeoEnrichmentService
{
    public async Task<GeoData?> EnrichAsync(
        string? ipAddress,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var proxyGeoData = GetTrustedProxyGeoData(request);
        if (proxyGeoData is not null)
        {
            return proxyGeoData;
        }

        var lookupBaseUrl = configuration["Geo:IpLookupBaseUrl"];
        if (string.IsNullOrWhiteSpace(lookupBaseUrl)
            || !Uri.TryCreate(lookupBaseUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttps && !environment.IsDevelopment()))
        {
            return null;
        }

        if (!IPAddress.TryParse(ipAddress, out var parsedIp) || IPAddress.IsLoopback(parsedIp))
        {
            return null;
        }

        try
        {
            var lookupUri = new Uri(baseUri, $"{Uri.EscapeDataString(ipAddress!)}/json/");
            using var response = await httpClientFactory.CreateClient("geo-lookup")
                .GetAsync(lookupUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Geo lookup returned HTTP status {StatusCode}.", response.StatusCode);
                return null;
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            var root = document.RootElement;

            return new GeoData(
                ReadString(root, "country_name"),
                ReadString(root, "region"),
                ReadString(root, "city"),
                ReadDouble(root, "latitude"),
                ReadDouble(root, "longitude"));
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Geo lookup request failed.");
            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Geo lookup response was not valid JSON.");
            return null;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Geo lookup timed out.");
            return null;
        }
    }

    private GeoData? GetTrustedProxyGeoData(HttpRequest request)
    {
        var trustedProxyAddresses = configuration
            .GetSection("Geo:TrustedProxyAddresses")
            .Get<string[]>() ?? [];
        var remoteAddress = request.HttpContext.Connection.RemoteIpAddress;
        if (remoteAddress is null || !trustedProxyAddresses.Any(address =>
                IPAddress.TryParse(address, out var trustedAddress)
                && trustedAddress.Equals(remoteAddress)))
        {
            return null;
        }

        var country = request.Headers["CF-IPCountry"].FirstOrDefault();
        var region = request.Headers["CF-Region"].FirstOrDefault();
        var city = request.Headers["CF-IPCity"].FirstOrDefault();
        var latitude = ParseHeaderDouble(request.Headers["CF-IPLatitude"].FirstOrDefault());
        var longitude = ParseHeaderDouble(request.Headers["CF-IPLongitude"].FirstOrDefault());

        return country is null && region is null && city is null && latitude is null && longitude is null
            ? null
            : new GeoData(country, region, city, latitude, longitude);
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static double? ReadDouble(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.TryGetDouble(out var value)
            ? value
            : null;

    private static double? ParseHeaderDouble(string? value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
}
