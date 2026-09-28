using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartPantry.Products;

public interface IProductAppService :
    ICrudAppService< //Defines CRUD methods
        ProductDto, //Used to show products
        Guid, //Primary key of the product entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateProductDto> //Used to create/update a product
{

}
