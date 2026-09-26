using System;
using System.Threading.Tasks;
using SmartPantry.Products;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry;

public class SmartPantryDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Product, Guid> _productRepository;

    public SmartPantryDataSeederContributor(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _productRepository.GetCountAsync() <= 0)
        {
            await _productRepository.InsertAsync(
                new Product
                {
                    Barcode = "8412345678901",
                    Name = "Bebida de Almendras Sin Azúcar",
                    Brand = "AlmondGreen",
                    NutriScore = "B",
                    NovaGroup = 4,
                },
                autoSave: true
            );

            await _productRepository.InsertAsync(
                new Product
                {
                    Barcode = "7791234567890",
                    Name = "Galletas de Avena Integrales",
                    Brand = "NaturaLife",
                    NutriScore = "A",
                    NovaGroup = 3,
                },
                autoSave: true
            );
        }
    }
}

