using SmartPantry.Products;
using SmartPantry.Products.Barcode;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Volo.Abp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartPantry.HttpApi.Host;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        string url = $"api/v3/product/{barcode}";

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }

        if (response.StatusCode == (HttpStatusCode)429)
        {
            throw new UserFriendlyException("Se ha superado el límite de solicitudes a Open Food Facts. Intente más tarde.");
        }

        response.EnsureSuccessStatusCode();

        var externalApiResponse = await response.Content.ReadFromJsonAsync<OpenFoodFactsResponseDto>();

        if (externalApiResponse == null || externalApiResponse.Product == null)
        {
            return null;
        }

        var p = externalApiResponse.Product;

        string nutrients = string.Empty;
        if (p.Nutriments != null)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (GetDouble(p.Nutriments, "energy-kcal_100g") is double kcal)
                parts.Add($"Energía: {kcal} kcal/100g");
            if (GetDouble(p.Nutriments, "fat_100g") is double fat)
                parts.Add($"Grasas: {fat} g/100g");
            if (GetDouble(p.Nutriments, "sugars_100g") is double sugars)
                parts.Add($"Azúcares: {sugars} g/100g");
            if (GetDouble(p.Nutriments, "proteins_100g") is double proteins)
                parts.Add($"Proteínas: {proteins} g/100g");
            if (GetDouble(p.Nutriments, "salt_100g") is double salt)
                parts.Add($"Sal: {salt} g/100g");
            nutrients = string.Join(", ", parts);
        }
        else
        {
            nutrients = "No se tiene información nutricional sobre el producto";
        }

        return new ExternalProductDto
        {
            Barcode = barcode,
            Name = p.ProductName ?? "No se tiene información sobre el nombre del producto",
            Brand = p.Brands ?? "No se tiene información sobre la marca del producto",
            Quantity = p.Quantity ?? "No se tiene información sobre la cantidad del producto",
            ImageUrl = p.ImageFrontUrl ?? "No se tiene información sobre la imagen del producto",
            Nutrients = nutrients
        };
    }

    private static double? GetDouble(
        System.Collections.Generic.Dictionary<string, JsonElement> dict,
        string key)
    {
        if (dict.TryGetValue(key, out var element) &&
            element.ValueKind == JsonValueKind.Number &&
            element.TryGetDouble(out var value))
        {
            return value;
        }
        return null;
    }
}

internal class OpenFoodFactsResponseDto
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("product")]
    public OpenFoodFactsProductDto? Product { get; set; }
}

internal class OpenFoodFactsProductDto
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("brands")]
    public string? Brands { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("image_front_url")]
    public string? ImageFrontUrl { get; set; }

    [JsonPropertyName("nutriments")]
    public System.Collections.Generic.Dictionary<string, JsonElement>? Nutriments { get; set; }
}