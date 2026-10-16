using System.Threading.Tasks;

namespace SmartPantry 
{
    public interface IExternalProductCatalogClient
    {
        Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
    }
}