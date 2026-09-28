using System;
using System.Threading.Tasks;
using SmartPantry.Products;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SmartPantry;

public class SmartPantryDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IGuidGenerator _guidGenerator;

    public SmartPantryDataSeederContributor(
        IRepository<Product, Guid> productRepository,
        IGuidGenerator guidGenerator)
    {
        _productRepository = productRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _productRepository.GetCountAsync() <= 0)
        {
            await _productRepository.InsertAsync(
                new Product(
                    _guidGenerator.Create(),
                    "8412345678901",
                    "Bebida de Almendras Sin Azúcar",
                    "AlmondGreen",
                    "B",
                    4
                ),
                autoSave: true
            );

            await _productRepository.InsertAsync(
                new Product(
                    _guidGenerator.Create(),
                    "7791234567890",
                    "Galletas de Avena Integrales",
                    "NaturaLife",
                    "A",
                    3
                ),
                autoSave: true
            );
        }
    }
}

