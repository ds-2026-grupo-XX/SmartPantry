using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartPantry.HttpApi.Host.Clients;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string CodigoDeBarras)
    {
        var url = $"api/v3/product/{CodigoDeBarras}.json";

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == (HttpStatusCode)429)
        {
            throw new UserFriendlyException("Se ha superado el limite de solicitudes a la API Externa. Intente mas tarde");
        }
        if ((int)response.StatusCode >= 500)
        {
            throw new UserFriendlyException("Error en la API Externa. Intente mas tarde");
        }

        // Si no se encuentra en la API externa, devolvemos el DTO con Encontrado = false (Estado de la consulta)
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new ExternalProductDto
            {
                CodigoDeBarras = CodigoDeBarras,
                Encontrado = false
            };
        }

        response.EnsureSuccessStatusCode();

        // Deserializamos usando clases internas exclusivas para el JSON externo
        var apiResponse = await response.Content.ReadFromJsonAsync<OpenFoodFactsProductResponse>();

        if (apiResponse?.Product == null)
        {
            return new ExternalProductDto
            {
                CodigoDeBarras = CodigoDeBarras,
                Encontrado = false
            };
        }

        // Traducimos el JSON externo a nuestro DTO interno mapeando los campos reales de la API
        return new ExternalProductDto
        {
            CodigoDeBarras = CodigoDeBarras,
            Encontrado = true, 
            NombreVisible = apiResponse.Product.NombreVisible,
            Marca = apiResponse.Product.Marca,
            Imagen = apiResponse.Product.Imagen
        };
    }

    // Clases internas para deserializar el JSON de la API con los nombres reales de Open Food Facts
    private class OpenFoodFactsProductResponse
    {
        [JsonPropertyName("product")]
        public OpenFoodFactsProductDetails? Product { get; set; }
    }

    private class OpenFoodFactsProductDetails
    {
        [JsonPropertyName("product_name")]
        public string? NombreVisible { get; set; }

        [JsonPropertyName("brands")]
        public string? Marca { get; set; }

        [JsonPropertyName("image_front_url")]
        public string? Imagen { get; set; }
    }
}