using System;
using Shouldly;
using Xunit;

namespace SmartPantry;

public class ProductTest
{
    // Prueba de dominio 1: normaliza los datos de texto
    [Fact]
    public void NormalizaTextos()
    {
        var id = Guid.NewGuid();
        var barcodeConEspacios = " 123456789 ";
        var nombreConEspacios = "   Maní pelado sin sal   ";
        var marcaConEspacios = "  MarcaX  ";

        var product = new Product(id, barcodeConEspacios, nombreConEspacios, marcaConEspacios, "A", 1);

        product.Barcode.ShouldBe("123456789");
        product.Name.ShouldBe("Maní pelado sin sal");
        product.Brand.ShouldBe("MarcaX");
    }

    // Prueba de dominio 2: rechaza un valor obligatorio vacío o con espacios
    [Fact]
    public void RechazaEspaciosVacios()
    {
        var id = Guid.NewGuid();
        var nombreInvalido = "   "; 
        Should.Throw<ArgumentException>(() =>
        {
            new Product(id, "123456", nombreInvalido, "MarcaX", "A", 1);
        });
    }
}