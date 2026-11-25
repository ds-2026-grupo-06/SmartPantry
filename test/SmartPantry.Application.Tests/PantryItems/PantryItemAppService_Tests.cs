using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.PantryItems;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class PantryItemAppService_Tests : SmartPantryApplicationTestBase<SmartPantryApplicationTestModule>
{
    private readonly IPantryItemAppService _pantryItemAppService;
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;

    // Usuario autenticado configurado en FakeCurrentPrincipalAccessor
    private static readonly Guid UsuarioAutenticadoId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private static readonly Guid OtroUsuarioId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public PantryItemAppService_Tests()
    {
        _pantryItemAppService = GetRequiredService<IPantryItemAppService>();
        _pantryItemRepository = GetRequiredService<IRepository<PantryItem, Guid>>();
    }

    [Fact]
    public async Task CicloCompleto_Crear_Listar_Actualizar_MarcarConsumido_Eliminar()
    {
        var fechaInicial = new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Local);

        // 1. CREAR ÍTEM PROPIO
        var creado = await _pantryItemAppService.CreateAsync(new CreateUpdatePantryItemDto
        {
            NombreProducto = "Leche Descremada",
            Estado = EstadoPantryItem.Disponible,
            FechaVencimiento = fechaInicial,
            Cantidad = 2,
            Unidad = "litros"
        });

        creado.Id.ShouldNotBe(Guid.Empty);
        creado.IdPropietario.ShouldBe(UsuarioAutenticadoId);
        creado.NombreProducto.ShouldBe("Leche Descremada");
        creado.Cantidad.ShouldBe(2);
        creado.Unidad.ShouldBe("litros");

        // 2. LISTAR DE FORMA PAGINADA
        var listado = await _pantryItemAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        listado.TotalCount.ShouldBeGreaterThan(0);
        listado.Items.ShouldContain(i => i.Id == creado.Id && i.NombreProducto == "Leche Descremada");

        // 3. ACTUALIZAR CANTIDAD, UNIDAD Y FECHA DE VENCIMIENTO
        var nuevaFecha = fechaInicial.AddDays(10);
        var actualizado = await _pantryItemAppService.UpdateAsync(creado.Id, new CreateUpdatePantryItemDto
        {
            NombreProducto = "Leche Descremada",
            Estado = EstadoPantryItem.Disponible,
            FechaVencimiento = nuevaFecha,
            Cantidad = 5,
            Unidad = "unidades"
        });

        actualizado.Cantidad.ShouldBe(5);
        actualizado.Unidad.ShouldBe("unidades");
        actualizado.FechaVencimiento.ShouldBe(nuevaFecha);

        // 4. MARCAR COMO CONSUMIDO
        var consumido = await _pantryItemAppService.ActualizarEstadoAsync(creado.Id, EstadoPantryItem.Consumido);
        consumido.Estado.ShouldBe(EstadoPantryItem.Consumido);

        // 5. ELIMINAR
        await _pantryItemAppService.EliminarItemAsync(creado.Id);

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _pantryItemAppService.GetAsync(creado.Id);
        });
    }

    [Fact]
    public async Task ItemDeOtroUsuario_NoDebeVerseNiModificarseNiEliminarse()
    {
        var itemAjenoId = Guid.NewGuid();
        var fechaOriginal = new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Local);

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemAjenoId,
                OtroUsuarioId,
                "Café Molido Ajeno",
                EstadoPantryItem.Disponible,
                fechaOriginal,
                1,
                "paquete"
            ), autoSave: true);
        });

        // 1. El listado paginado no debe incluir ítems de otro usuario
        var listado = await _pantryItemAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        listado.Items.ShouldNotContain(i => i.Id == itemAjenoId);

        // 2. Leer un ítem ajeno debe responder 404 (EntityNotFoundException)
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _pantryItemAppService.GetAsync(itemAjenoId);
        });

        // 3. Modificar un ítem ajeno debe responder 404 (EntityNotFoundException)
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _pantryItemAppService.UpdateAsync(itemAjenoId, new CreateUpdatePantryItemDto
            {
                NombreProducto = "Intento Modificar",
                Estado = EstadoPantryItem.Disponible,
                FechaVencimiento = fechaOriginal.AddDays(5),
                Cantidad = 10,
                Unidad = "kg"
            });
        });

        // 4. Marcar como consumido un ítem ajeno debe responder 404 (EntityNotFoundException)
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _pantryItemAppService.ActualizarEstadoAsync(itemAjenoId, EstadoPantryItem.Consumido);
        });

        // 5. Eliminar un ítem ajeno debe responder 404 (EntityNotFoundException)
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _pantryItemAppService.EliminarItemAsync(itemAjenoId);
        });

        // Verificar que el ítem ajeno permanece intacto en la base de datos
        await WithUnitOfWorkAsync(async () =>
        {
            var itemEnBase = await _pantryItemRepository.GetAsync(itemAjenoId);
            itemEnBase.NombreProducto.ShouldBe("Café Molido Ajeno");
            itemEnBase.Estado.ShouldBe(EstadoPantryItem.Disponible);
            itemEnBase.Cantidad.ShouldBe(1);
        });
    }

    [Fact]
    public async Task ValidacionServidor_CantidadNegativaOUnidadMuyLarga_DebeRechazar()
    {
        await Should.ThrowAsync<AbpValidationException>(async () =>
        {
            await _pantryItemAppService.CreateAsync(new CreateUpdatePantryItemDto
            {
                NombreProducto = "Arroz",
                Estado = EstadoPantryItem.Disponible,
                Cantidad = -1,
                Unidad = "kg"
            });
        });

        await Should.ThrowAsync<AbpValidationException>(async () =>
        {
            await _pantryItemAppService.CreateAsync(new CreateUpdatePantryItemDto
            {
                NombreProducto = "Arroz",
                Estado = EstadoPantryItem.Disponible,
                Cantidad = 1,
                Unidad = new string('x', 50)
            });
        });
    }
}
