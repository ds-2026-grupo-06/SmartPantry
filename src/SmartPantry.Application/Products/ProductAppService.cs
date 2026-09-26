using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

public class ProductsAppService :
    CrudAppService<
        Product, //The Product entity
        ProductDto, //Used to show products
        Guid, //Primary key of the product entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateProductDto>, //Used to create/update a product
    IProductAppService //implement the IProductAppService
{
    public ProductsAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {

    }
}
