using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

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

    public ProductsAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
        _repository = repository;
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
        // 1. Buscamos la entidad en la base de datos
        var product = await _repository.GetAsync(id);

        // 2. Ejecutamos el método del dominio (valida y normaliza)
        product.UpdateDetails(input.Name, input.Brand, input.NutriScore, input.NovaGroup);

        // 3. Persistimos los cambios
        await _repository.UpdateAsync(product);

        // 4. Retornamos el DTO
        return ObjectMapper.Map<Product, ProductDto>(product);
    }
}