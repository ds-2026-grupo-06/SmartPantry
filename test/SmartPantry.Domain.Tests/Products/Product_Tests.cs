using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SmartPantry.Products;

public class Product_Tests
{
	[Fact]
	public void ModificacionValida_DebeConservarNormalizaciones()
	{
		// Arrange
		var product = new Product(
			Guid.NewGuid(),
			"7791234567890",
			"Arroz Original",
			"Marca Original",
			"A",
			1
		);

		// Act: Enviamos textos con espacios y NutriScore en minúscula
		product.UpdateDetails(
			"   Arroz Doble Carolina   ",
			"   Gallo Oro   ",
			"b",
			2
		);

		// Assert: Comprobamos que recortó espacios y pasó a mayúscula
		product.Name.ShouldBe("Arroz Doble Carolina");
		product.Brand.ShouldBe("Gallo Oro");
		product.NutriScore.ShouldBe("B");
		product.NovaGroup.ShouldBe(2);
	}

	[Fact]
	public void ModificacionInvalida_DebeSerRechazada_YDejarEntidadEnEstadoAnterior()
	{
		// Arrange
		var product = new Product(
			Guid.NewGuid(),
			"7791234567890",
			"Arroz Original",
			"Marca Original",
			"A",
			1
		);

		// Act & Assert: Intentar asignar un NovaGroup inválido (5) debe disparar BusinessException
		Should.Throw<BusinessException>(() =>
		{
			// Intentamos cambiar el nombre a algo nuevo, pero con grupo NOVA inválido
			product.UpdateDetails(
				"Nombre Invalido",
				"Marca Invalida",
				"c",
				5
			);
		});

		// Assert de invariante: La entidad debe mantener el estado previo intacto
		product.Name.ShouldBe("Arroz Original");
		product.Brand.ShouldBe("Marca Original");
		product.NutriScore.ShouldBe("A");
		product.NovaGroup.ShouldBe(1);
	}
}