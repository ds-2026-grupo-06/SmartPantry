using System.Threading.Tasks;
using SmartPantry.Products.Barcode; 

namespace SmartPantry.Products;

public interface IExternalProductCatalogClient
{
    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
}