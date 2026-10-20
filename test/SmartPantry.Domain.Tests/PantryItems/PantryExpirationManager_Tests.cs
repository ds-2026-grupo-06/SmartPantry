using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SmartPantry.PantryItems;

/// <summary>
/// Pruebas automatizadas del servicio de procesamiento de vencimientos (RF-15 - TP08).
/// 
/// Criterio de fecha explícito:
/// Tanto el servicio de dominio como las pruebas utilizan FECHA LOCAL (DateTimeKind.Local / .Date),
/// evaluando días calendario completos sin depender del reloj en tiempo real ni esperar al temporizador.
/// </summary>
public abstract class PantryExpirationManager_Tests<TStartupModule> : SmartPantryDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly PantryExpirationManager _expirationManager;
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IRepository<PantryItemWarning, Guid> _warningRepository;

    // Fecha controlada fija en hora local para que las pruebas sean deterministas y repetibles
    private static readonly DateTime FechaControladaLocal = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Local);
    private static readonly Guid PropietarioPruebaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    protected PantryExpirationManager_Tests()
    {
        _expirationManager = GetRequiredService<PantryExpirationManager>();
        _pantryItemRepository = GetRequiredService<IRepository<PantryItem, Guid>>();
        _warningRepository = GetRequiredService<IRepository<PantryItemWarning, Guid>>();
    }

    [Fact]
    public async Task ProcesarVencimiento_ItemDentroDelUmbral_DebeCrearAdvertenciaActiva()
    {
        var itemId = Guid.NewGuid();
        var fechaVencimiento = FechaControladaLocal.AddDays(2); // Dentro del umbral de 3 días

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Leche Entera",
                EstadoPantryItem.Disponible,
                fechaVencimiento
            ), autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencia = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
            advertencia.ShouldNotBeNull();
            advertencia.EstaActivo.ShouldBeTrue();
            advertencia.TipoAviso.ShouldBe(PantryExpirationManager.TipoAvisoVencimiento);
            advertencia.FechaVencimiento.Date.ShouldBe(fechaVencimiento.Date);
            advertencia.Mensaje.ShouldContain("próximo a vencer");
        });
    }

    [Fact]
    public async Task ProcesarVencimiento_ItemEnDiaLimiteExacto_DebeCrearAdvertenciaActiva()
    {
        var itemId = Guid.NewGuid();
        // Día límite exacto: exactamente 3 días (UmbralDias) desde la fecha controlada local
        var fechaDiaLimite = FechaControladaLocal.AddDays(PantryExpirationManager.UmbralDias);

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Queso Untable",
                EstadoPantryItem.Disponible,
                fechaDiaLimite
            ), autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencia = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
            advertencia.ShouldNotBeNull();
            advertencia.EstaActivo.ShouldBeTrue();
            advertencia.FechaVencimiento.Date.ShouldBe(fechaDiaLimite.Date);
            advertencia.Mensaje.ShouldContain("vence en 3 días");
        });
    }

    [Fact]
    public async Task ProcesarVencimiento_ItemFueraDelUmbral_NoDebeCrearAdvertencia()
    {
        var itemId = Guid.NewGuid();
        // Fuera del umbral: 4 días (mayor que UmbralDias = 3)
        var fechaFueraDeUmbral = FechaControladaLocal.AddDays(PantryExpirationManager.UmbralDias + 1);

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Fideos Secos",
                EstadoPantryItem.Disponible,
                fechaFueraDeUmbral
            ), autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencia = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
            advertencia.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ProcesarVencimiento_ItemVencido_DebeCrearAdvertenciaActivaDeVencido()
    {
        var itemId = Guid.NewGuid();
        var fechaVencida = FechaControladaLocal.AddDays(-2);

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Yogur Bebible",
                EstadoPantryItem.Disponible,
                fechaVencida
            ), autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencia = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
            advertencia.ShouldNotBeNull();
            advertencia.EstaActivo.ShouldBeTrue();
            advertencia.FechaVencimiento.Date.ShouldBe(fechaVencida.Date);
            advertencia.Mensaje.ShouldContain("VENCIDO");
        });
    }

    [Fact]
    public async Task ProcesarVencimiento_ItemSinFecha_NoDebeCrearAdvertencia()
    {
        var itemId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Azúcar Blanca",
                EstadoPantryItem.Disponible,
                null
            ), autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencia = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
            advertencia.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ProcesarVencimiento_ItemConsumido_NoDebeCrearAdvertencia()
    {
        var itemId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Pan Lactal",
                EstadoPantryItem.Consumido,
                FechaControladaLocal.AddDays(1) // Fecha dentro del umbral, pero ya fue consumido
            ), autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencia = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
            advertencia.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ProcesarVencimiento_EjecutadoDosVecesSobreMismosDatos_DebeDemostrarUnaSolaAdvertenciaPorItem()
    {
        var itemProximoId = Guid.NewGuid();
        var itemVencidoId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemProximoId,
                PropietarioPruebaId,
                "Manteca",
                EstadoPantryItem.Disponible,
                FechaControladaLocal.AddDays(1)
            ), autoSave: true);

            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemVencidoId,
                PropietarioPruebaId,
                "Crema de Leche",
                EstadoPantryItem.Disponible,
                FechaControladaLocal.AddDays(-1)
            ), autoSave: true);

            // Primera ejecución
            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);

            // Segunda ejecución inmediata sobre los mismos datos (en la misma unidad de trabajo)
            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        // Tercera ejecución en otra unidad de trabajo independiente
        await WithUnitOfWorkAsync(async () =>
        {
            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertenciasProximo = await _warningRepository.GetListAsync(w => w.PantryItemId == itemProximoId);
            advertenciasProximo.Count.ShouldBe(1);
            advertenciasProximo.Single().EstaActivo.ShouldBeTrue();

            var advertenciasVencido = await _warningRepository.GetListAsync(w => w.PantryItemId == itemVencidoId);
            advertenciasVencido.Count.ShouldBe(1);
            advertenciasVencido.Single().EstaActivo.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task ProcesarVencimiento_CambioDeFecha_DebeActualizarDesactivarYReactivarLaMismaAdvertencia()
    {
        var itemId = Guid.NewGuid();
        var fechaInicial = FechaControladaLocal.AddDays(2);
        var fechaActualizadaVencida = FechaControladaLocal.AddDays(-1);
        var fechaFueraDelUmbral = FechaControladaLocal.AddDays(15);
        var fechaReactivacion = FechaControladaLocal.AddDays(PantryExpirationManager.UmbralDias);

        Guid advertenciaOriginalId = Guid.Empty;

        // 1. Creación inicial (vence en 2 días -> activa)
        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Dulce de Leche",
                EstadoPantryItem.Disponible,
                fechaInicial
            ), autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencias = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
            advertencias.Count.ShouldBe(1);

            var advertencia = advertencias.Single();
            advertenciaOriginalId = advertencia.Id;
            advertencia.EstaActivo.ShouldBeTrue();
            advertencia.FechaVencimiento.Date.ShouldBe(fechaInicial.Date);
            advertencia.Mensaje.ShouldContain("próximo a vencer");
        });

        // 2. Actualización de fecha (pasa a vencido -> actualiza la MISMA advertencia)
        await WithUnitOfWorkAsync(async () =>
        {
            var item = await _pantryItemRepository.GetAsync(itemId);
            item.ActualizarFechaVencimiento(fechaActualizadaVencida);
            await _pantryItemRepository.UpdateAsync(item, autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencias = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
            advertencias.Count.ShouldBe(1);

            var advertencia = advertencias.Single();
            advertencia.Id.ShouldBe(advertenciaOriginalId);
            advertencia.EstaActivo.ShouldBeTrue();
            advertencia.FechaVencimiento.Date.ShouldBe(fechaActualizadaVencida.Date);
            advertencia.Mensaje.ShouldContain("VENCIDO");
        });

        // 3. Desactivación (cambia la fecha fuera del umbral -> desactiva la MISMA advertencia)
        await WithUnitOfWorkAsync(async () =>
        {
            var item = await _pantryItemRepository.GetAsync(itemId);
            item.ActualizarFechaVencimiento(fechaFueraDelUmbral);
            await _pantryItemRepository.UpdateAsync(item, autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencias = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
            advertencias.Count.ShouldBe(1);

            var advertencia = advertencias.Single();
            advertencia.Id.ShouldBe(advertenciaOriginalId);
            advertencia.EstaActivo.ShouldBeFalse();
        });

        // 4. Eventual reactivación (vuelve a entrar en el umbral -> reactiva la MISMA advertencia sin duplicar)
        await WithUnitOfWorkAsync(async () =>
        {
            var item = await _pantryItemRepository.GetAsync(itemId);
            item.ActualizarFechaVencimiento(fechaReactivacion);
            await _pantryItemRepository.UpdateAsync(item, autoSave: true);

            await _expirationManager.ProcesarVencimiento(FechaControladaLocal);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var advertencias = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
            advertencias.Count.ShouldBe(1);

            var advertencia = advertencias.Single();
            advertencia.Id.ShouldBe(advertenciaOriginalId);
            advertencia.EstaActivo.ShouldBeTrue();
            advertencia.FechaVencimiento.Date.ShouldBe(fechaReactivacion.Date);
            advertencia.Mensaje.ShouldContain("próximo a vencer");
        });
    }

    [Fact]
    public async Task IndiceUnico_InsertarDosAdvertenciasConMismaClaveLogica_DebeSerImpedidoPorElEsquema()
    {
        var itemId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _pantryItemRepository.InsertAsync(new PantryItem(
                itemId,
                PropietarioPruebaId,
                "Harina 0000",
                EstadoPantryItem.Disponible,
                FechaControladaLocal.AddDays(1)
            ), autoSave: true);

            await _warningRepository.InsertAsync(new PantryItemWarning(
                Guid.NewGuid(),
                itemId,
                PantryExpirationManager.TipoAvisoVencimiento,
                "Primera advertencia",
                FechaControladaLocal.AddDays(1)
            ), autoSave: true);
        });

        // Intentar insertar una segunda advertencia para el mismo ítem y tipo de aviso debe fallar por el índice único
        await Should.ThrowAsync<Exception>(async () =>
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _warningRepository.InsertAsync(new PantryItemWarning(
                    Guid.NewGuid(),
                    itemId,
                    PantryExpirationManager.TipoAvisoVencimiento,
                    "Segunda advertencia duplicada",
                    FechaControladaLocal.AddDays(1)
                ), autoSave: true);
            });
        });
    }
}
