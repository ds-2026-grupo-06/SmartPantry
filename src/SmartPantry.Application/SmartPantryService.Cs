using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry;

public class SmartPantryService : ApplicationService, ISmartPantryService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public SmartPantryService(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> GetListAsync()
    {
        var items = await _productRepository.GetListAsync();
        return items
            .Select(item => new ProductDto
            {
                Id = item.Id,
                Barcode = item.Barcode,
                Name = item.Name,
                Brand = item.Brand,
                NutriScore = item.NutriScore,
                NovaGroup = item.NovaGroup
            }).ToList();
    }

    public async Task<ProductDto> CreateAsync(string barcode, string name, string brand, string nutriScore, int novaGroup)
    {
        var product = await _productRepository.InsertAsync(
            new Product
            {
                Barcode = barcode,
                Name = name,
                Brand = brand,
                NutriScore = nutriScore,
                NovaGroup = novaGroup
            }
        );

        return new ProductDto
        {
            Id = product.Id,
            Barcode = product.Barcode,
            Name = product.Name,
            Brand = product.Brand,
            NutriScore = product.NutriScore,
            NovaGroup = product.NovaGroup
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        await _productRepository.DeleteAsync(id);
    }


}
