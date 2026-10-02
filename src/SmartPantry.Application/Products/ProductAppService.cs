using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using SmartPantry.Products.Barcode;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Products;

[ExposeServices(typeof(IProductAppService), typeof(ProductsAppService))]
public class ProductsAppService :
    CrudAppService<
        Product,
        ProductDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateProductDto>,
    IProductAppService
{
    private readonly IRepository<Product, Guid> _repository;
    private readonly IExternalProductCatalogClient _externalCatalogClient; 

    public ProductsAppService(
        IRepository<Product, Guid> repository,
        IExternalProductCatalogClient externalCatalogClient) 
        : base(repository)
    {
        _repository = repository;
        _externalCatalogClient = externalCatalogClient;
    }

    public override async Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        var product = new Product(
            GuidGenerator.Create(),
            input.Barcode,
            input.Name,
            input.Brand,
            input.NutriScore,
            input.NovaGroup
        );

        await _repository.InsertAsync(product);

        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input)
    {
        var product = await _repository.GetAsync(id);

        product.UpdateDetails(input.Name, input.Brand, input.NutriScore, input.NovaGroup);

        await _repository.UpdateAsync(product);

        return ObjectMapper.Map<Product, ProductDto>(product);
    }


    public async Task<ExternalProductDto> GetByBarcodeAsync(GetProductByBarcodeDto input)
    {
        var externalProduct = await _externalCatalogClient.GetByBarcodeAsync(input.Barcode);

        if (externalProduct == null)
        {
            throw new EntityNotFoundException(typeof(ExternalProductDto), input.Barcode);
        }

        return externalProduct;
    }
}