using System;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using SmartPantry.Products.Barcode;

namespace SmartPantry.Products;

public interface IProductAppService :
    ICrudAppService< //Defines CRUD methods
        ProductDto, //Used to show products
        Guid, //Primary key of the product entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateProductDto> //Used to create/update a product
{
    //Contrado de servicio para obtener un producto por su código de barras
    Task<ExternalProductDto> GetByBarcodeAsync(GetProductByBarcodeDto input);
}
