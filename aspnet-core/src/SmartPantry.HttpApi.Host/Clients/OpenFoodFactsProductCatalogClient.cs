using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
        // Armamos la ruta de la API v3 de Open Food Facts (según lo explorado en la consola)
        var url = $"api/v3/product/{CodigoDeBarras}.json";

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null; 
        }

        response.EnsureSuccessStatusCode();

        // Deserializamos usando clases internas exclusivas para el JSON externo
        var apiResponse = await response.Content.ReadFromJsonAsync<OpenFoodFactsProductResponse>();

        if (apiResponse?.Product == null)
        {
            return null;
        }

        // Traducimos el JSON externo a nuestro DTO interno limpio (respetando RF-09 si faltan campos)
        return new ExternalProductDto
        {
            CodigoDeBarras = CodigoDeBarras,
            NombreVisible = apiResponse.Product.NombreVisible,
           // Brand = apiResponse.Product.Brands,
            Imagen = apiResponse.Product.Imagen
        };
    }

    // Clases internas para deserializar el JSON de la API (no se exponen al resto del sistema)
    private class OpenFoodFactsProductResponse
    {
        public OpenFoodFactsProductDetails? Product { get; set; }
    }

    private class OpenFoodFactsProductDetails
    {
        public string? NombreVisible{ get; set; }
       // public string? Brands { get; set; }
        public string? Imagen { get; set; }
        //public string? ImageFrontUrl { get; set; }
    }
}