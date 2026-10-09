using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.Products.Barcode;
using Volo.Abp;
using Xunit;

namespace SmartPantry.Products;

public class OpenFoodFactsProductCatalogClient_Tests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public HttpRequestMessage? LastRequest { get; private set; }

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        public MockHttpMessageHandler(HttpStatusCode statusCode, string contentJson = "")
            : this(_ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(contentJson, Encoding.UTF8, "application/json")
            })
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_handler(request));
        }
    }

    private static HttpClient CreateHttpClient(MockHttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://world.openfoodfacts.org/")
        };
    }

    [Fact]
    public async Task GetByBarcodeAsync_DebeRealizarConsultaGetConRutaCorrecta_YRetornarProductoMapeado()
    {
        // Arrange
        var barcode = "3017620422003";
        var jsonResponse = """
        {
            "status": "success",
            "product": {
                "code": "3017620422003",
                "product_name": "Nutella",
                "brands": "Ferrero",
                "quantity": "400 g",
                "image_front_url": "https://images.openfoodfacts.org/nutella.jpg",
                "nutriments": {
                    "energy-kcal_100g": 539,
                    "fat_100g": 30.9,
                    "sugars_100g": 56.3,
                    "proteins_100g": 6.3,
                    "salt_100g": 0.107
                }
            }
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act
        var result = await client.GetByBarcodeAsync(barcode);

        // Assert: Verificación de la consulta HTTP y la ruta al proveedor
        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri.ShouldNotBeNull();
        handler.LastRequest.RequestUri.ToString().ShouldBe($"https://world.openfoodfacts.org/api/v3/product/{barcode}");

        // Assert: Verificación del mapeo completo de datos
        result.ShouldNotBeNull();
        result.Barcode.ShouldBe(barcode);
        result.Name.ShouldBe("Nutella");
        result.Brand.ShouldBe("Ferrero");
        result.Quantity.ShouldBe("400 g");
        result.ImageUrl.ShouldBe("https://images.openfoodfacts.org/nutella.jpg");
        result.Nutrients.ShouldContain("Energía: 539 kcal/100g");
        result.Nutrients.ShouldContain("Grasas: 30.9 g/100g");
        result.Nutrients.ShouldContain("Azúcares: 56.3 g/100g");
        result.Nutrients.ShouldContain("Proteínas: 6.3 g/100g");
        result.Nutrients.ShouldContain("Sal: 0.107 g/100g");
    }

    [Fact]
    public async Task GetByBarcodeAsync_ConDatosAusentes_DebeMapearValoresPorDefecto()
    {
        // Arrange: Producto con campos ausentes o nulos (nombre, marca, cantidad, imagen y nutriments)
        var barcode = "123456789";
        var jsonResponse = """
        {
            "status": "success",
            "product": {
                "code": "123456789",
                "product_name": null,
                "brands": null,
                "quantity": null,
                "image_front_url": null,
                "nutriments": null
            }
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act
        var result = await client.GetByBarcodeAsync(barcode);

        // Assert: Mapeo de valores por defecto cuando los datos no están presentes
        result.ShouldNotBeNull();
        result.Barcode.ShouldBe(barcode);
        result.Name.ShouldBe("No se tiene información sobre el nombre del producto");
        result.Brand.ShouldBe("No se tiene información sobre la marca del producto");
        result.Quantity.ShouldBe("No se tiene información sobre la cantidad del producto");
        result.ImageUrl.ShouldBe("No se tiene información sobre la imagen del producto");
        result.Nutrients.ShouldBe("No se tiene información nutricional sobre el producto");
    }

    [Fact]
    public async Task GetByBarcodeAsync_CuandoObjetoProductoEsNull_DebeRetornarNull()
    {
        // Arrange: Respuesta de proveedor sin propiedad product
        var barcode = "123456789";
        var jsonResponse = """
        {
            "status": "product_not_found",
            "product": null
        }
        """;

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act
        var result = await client.GetByBarcodeAsync(barcode);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByBarcodeAsync_CuandoResponde404NotFound_DebeRetornarNull()
    {
        // Arrange: Simulación de respuesta 404 del proveedor
        var barcode = "0000000000000";
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "{\"status\": 0}");
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act
        var result = await client.GetByBarcodeAsync(barcode);

        // Assert
        result.ShouldBeNull();
        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.ToString().ShouldEndWith($"api/v3/product/{barcode}");
    }

    [Fact]
    public async Task GetByBarcodeAsync_CuandoResponde400BadRequest_DebeRetornarNull()
    {
        // Arrange: Simulación de respuesta 400 del proveedor
        var barcode = "codigo-invalido";
        var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, "{\"status\": \"bad_request\"}");
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act
        var result = await client.GetByBarcodeAsync(barcode);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByBarcodeAsync_CuandoResponde429TooManyRequests_DebeLanzarUserFriendlyException()
    {
        // Arrange: Simulación de respuesta 429 por límite de cuota superado
        var barcode = "3017620422003";
        var handler = new MockHttpMessageHandler((HttpStatusCode)429, "Too Many Requests");
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act & Assert
        var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await client.GetByBarcodeAsync(barcode);
        });

        ex.Message.ShouldBe("Se ha superado el límite de solicitudes a Open Food Facts. Intente más tarde.");
        ex.Code.ShouldBe(OpenFoodFactsProductCatalogClient.RateLimitErrorCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task GetByBarcodeAsync_CuandoProveedorRetornaError5xx_DebeIdentificarServicioComoNoDisponible(HttpStatusCode statusCode)
    {
        // Arrange: Simulación de error de servidor del proveedor (500, 502, 503, 504)
        var barcode = "3017620422003";
        var handler = new MockHttpMessageHandler(statusCode, "External Server Error");
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act & Assert: Debe identificar el servicio como no disponible (evitando llegar como 500 genérico no controlado)
        var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await client.GetByBarcodeAsync(barcode);
        });

        ex.Message.ShouldBe("El servicio externo no está disponible. Intente más tarde.");
        ex.Code.ShouldBe(OpenFoodFactsProductCatalogClient.ServiceUnavailableErrorCode);
    }

    [Fact]
    public async Task GetByBarcodeAsync_CuandoFallaConexionRed_DebeIdentificarServicioComoNoDisponible()
    {
        // Arrange: Simulación de fallo de red / comunicación
        var barcode = "3017620422003";
        var handler = new MockHttpMessageHandler(_ => throw new HttpRequestException("Connection reset by peer"));
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act & Assert
        var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await client.GetByBarcodeAsync(barcode);
        });

        ex.Message.ShouldBe("El servicio externo no está disponible. Intente más tarde.");
        ex.Code.ShouldBe(OpenFoodFactsProductCatalogClient.ServiceUnavailableErrorCode);
    }

    [Fact]
    public async Task GetByBarcodeAsync_CuandoOcurreTimeout_DebeIdentificarServicioComoNoDisponible()
    {
        // Arrange: Simulación de timeout
        var barcode = "3017620422003";
        var handler = new MockHttpMessageHandler(_ => throw new TaskCanceledException("The operation was canceled due to timeout."));
        var httpClient = CreateHttpClient(handler);
        var client = new OpenFoodFactsProductCatalogClient(httpClient);

        // Act & Assert
        var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await client.GetByBarcodeAsync(barcode);
        });

        ex.Message.ShouldBe("El servicio externo no está disponible. Intente más tarde.");
        ex.Code.ShouldBe(OpenFoodFactsProductCatalogClient.ServiceUnavailableErrorCode);
    }
}
