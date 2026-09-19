using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;
using SmartPantry.EntityFrameworkCore;

namespace SmartPantry;

public class SmartPantryServiceTest : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly ISmartPantryService _smartPantryService;
    private readonly IRepository<Product, Guid> _productRepository;

    public SmartPantryServiceTest()
    {
        _smartPantryService = GetRequiredService<ISmartPantryService>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
    }

    [Fact]
    public async Task CreaYRecuperaUnProductoPorId()
    {
        var createdDto = await _smartPantryService.CreateAsync("7791234567890", "Maní pelado sin sal", "MarcaX", "A", 1);

        createdDto.Id.ShouldNotBe(Guid.Empty);
        createdDto.Name.ShouldBe("Maní pelado sin sal");

        var productoEnBd = await _productRepository.GetAsync(createdDto.Id);
        productoEnBd.ShouldNotBeNull();
        productoEnBd.Id.ShouldBe(createdDto.Id);
        productoEnBd.Barcode.ShouldBe("7791234567890");
    }

    [Fact]
    public async Task NoPermiteCreacionConDtoInvalido()
    {
        await Should.ThrowAsync<ArgumentException>(async () =>
        {
            await _smartPantryService.CreateAsync("", "Maní pelado sin sal", "MarcaX", "A", 1);
        });
    }
}