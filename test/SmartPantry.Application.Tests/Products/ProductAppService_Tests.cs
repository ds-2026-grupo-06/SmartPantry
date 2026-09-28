using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace SmartPantry.Products;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class ProductAppService_Tests : SmartPantryApplicationTestBase<SmartPantryApplicationTestModule>
{
    private readonly IProductAppService _productAppService;

    public ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task CicloCompleto_Registrar_Listar_Modificar_Consultar_Eliminar()
    {
        // 1. REGISTRAR
        var createInput = new CreateUpdateProductDto
        {
            Barcode = "7791234567890",
            Name = "  Arroz Integral  ",
            Brand = "  Gallo  ",
            NutriScore = "a",
            NovaGroup = 1
        };

        var createdProduct = await _productAppService.CreateAsync(createInput);
        createdProduct.Id.ShouldNotBe(Guid.Empty);
        createdProduct.Name.ShouldBe("Arroz Integral");
        createdProduct.NutriScore.ShouldBe("A");

        // 2. CONSULTAR
        var fetchedProduct = await _productAppService.GetAsync(createdProduct.Id);
        fetchedProduct.ShouldNotBeNull();
        fetchedProduct.Barcode.ShouldBe("7791234567890");

        // 3. LISTAR
        var listResult = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        listResult.TotalCount.ShouldBeGreaterThan(0);
        listResult.Items.ShouldContain(p => p.Id == createdProduct.Id);

        // 4. MODIFICAR
        var updateInput = new CreateUpdateProductDto
        {
            Barcode = "7791234567890",
            Name = "  Arroz Doble Carolina  ",
            Brand = "Gallo Oro",
            NutriScore = "b",
            NovaGroup = 2
        };

        var updatedProduct = await _productAppService.UpdateAsync(createdProduct.Id, updateInput);
        updatedProduct.Name.ShouldBe("Arroz Doble Carolina");
        updatedProduct.NutriScore.ShouldBe("B");
        updatedProduct.NovaGroup.ShouldBe(2);

        // 5. ELIMINAR
        await _productAppService.DeleteAsync(createdProduct.Id);

        // 6. COMPROBACIÓN POST-ELIMINACIÓN (Debe lanzar EntityNotFoundException)
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _productAppService.GetAsync(createdProduct.Id);
        });
    }
}