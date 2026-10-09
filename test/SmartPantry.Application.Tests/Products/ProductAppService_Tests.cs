using NSubstitute;
using Shouldly;
using SmartPantry.Products.Barcode;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp;
using Xunit;

namespace SmartPantry.Products;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class ProductAppService_Tests : SmartPantryApplicationTestBase<SmartPantryApplicationTestModule>
{
    private readonly IProductAppService _productAppService;
    private readonly IExternalProductCatalogClient _externalCatalogClientMock;

    public ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
        _externalCatalogClientMock = GetRequiredService<IExternalProductCatalogClient>();
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

    // Pruebas de consulta de productos por código de barras, simulando la interacción con un cliente externo.
    [Fact]
    public async Task Should_Get_Product_By_Barcode_When_Exists()
    {
        // Arrange: Simulamos un código de barras válido
        var barcode = "3017620422003";
        var input = new GetProductByBarcodeDto { Barcode = barcode };

        var fakeExternalProduct = new ExternalProductDto
        {
            Barcode = barcode,
            Name = "Nutella",
            Brand = "Ferrero",
            Quantity = "400 g e",
            ImageUrl = "https://images.openfoodfacts.org/images/products/301/762/042/2003/front_en.879.400.jpg"
        };

        // Configuramos el mock para que devuelva el producto simulado cuando le pidan este código
        _externalCatalogClientMock
            .GetByBarcodeAsync(barcode)
            .Returns(Task.FromResult<ExternalProductDto?>(fakeExternalProduct));

        // Act: Ejecutamos el método del AppService
        var result = await _productAppService.GetByBarcodeAsync(input);

        // Assert: Verificamos que el resultado no sea nulo y coincida con los datos esperados
        result.ShouldNotBeNull();
        result.Barcode.ShouldBe(barcode);
        result.Name.ShouldBe("Nutella");
    }

    [Fact]
    public async Task Should_Return_Null_When_Product_Not_Exists()
    {
        // Arrange: Simulamos un código inexistente
        var barcode = "0000000000000";
        var input = new GetProductByBarcodeDto { Barcode = barcode };

        // Configuramos el mock para que devuelva null (producto no encontrado)
        _externalCatalogClientMock
            .GetByBarcodeAsync(barcode)
            .Returns(Task.FromResult<ExternalProductDto?>(null));

        // Act
        var result = await _productAppService.GetByBarcodeAsync(input);

        // Assert
        result.ShouldBeNull();
    }
    [Fact]
    public async Task Limite_De_Solicitudes()
    {
        // Arrange: Simulamos un código de barras válido
        var barcode = "3017620422003";
        var input = new GetProductByBarcodeDto { Barcode = barcode };
        // Configuramos el mock para que devuelva una excepción de límite de solicitudes
        _externalCatalogClientMock
            .GetByBarcodeAsync(barcode)
            .Returns<Task<ExternalProductDto?>>(_ => throw new UserFriendlyException(
                "Se ha superado el límite de solicitudes a Open Food Facts. Intente más tarde.",
                OpenFoodFactsProductCatalogClient.RateLimitErrorCode));
        // Act & Assert: Verificamos que se lance la excepción esperada
        var exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(input);
        });
        exception.Message.ShouldBe("Se ha superado el límite de solicitudes a Open Food Facts. Intente más tarde.");
        exception.Code.ShouldBe(OpenFoodFactsProductCatalogClient.RateLimitErrorCode);
    }
    [Fact]
    public async Task Servicio_Externo_No_Disponible()
    {
        // Arrange: Simulamos un código de barras válido
        var barcode = "3017620422003";
        var input = new GetProductByBarcodeDto { Barcode = barcode };
        // Configuramos el mock para que devuelva una excepción de servicio no disponible
        _externalCatalogClientMock
            .GetByBarcodeAsync(barcode)
            .Returns<Task<ExternalProductDto?>>(_ => throw new UserFriendlyException(
                "El servicio externo no está disponible. Intente más tarde.",
                OpenFoodFactsProductCatalogClient.ServiceUnavailableErrorCode));
        // Act & Assert: Verificamos que se lance la excepción esperada
        var exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(input);
        });
        exception.Message.ShouldBe("El servicio externo no está disponible. Intente más tarde.");
        exception.Code.ShouldBe(OpenFoodFactsProductCatalogClient.ServiceUnavailableErrorCode);
    }
}