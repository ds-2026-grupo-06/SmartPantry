using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SmartPantry.PantryItems;

public class PantryDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IGuidGenerator _guidGenerator;

    public PantryDataSeedContributor(
        IRepository<PantryItem, Guid> pantryItemRepository,
        IGuidGenerator guidGenerator)
    {
        _pantryItemRepository = pantryItemRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 1. Evitar duplicar datos si ya se ejecutó el seeding previamente
        if (await _pantryItemRepository.GetCountAsync() > 0)
        {
            return;
        }

        // Usamos un ID de propietario fijo persistido (sin depender de ICurrentUser)
        var sampleOwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        // Criterio de fecha: Fecha Local truncada al día (.Date)
        var today = DateTime.Now.Date;

        // 2. Insertar los casos de ejemplo locales requeridos por el TP
        // Caso 1: Ítem dentro del umbral (vence en 2 días)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            _guidGenerator.Create(),
            sampleOwnerId,
            "Leche Descremada",
            EstadoPantryItem.Disponible,
            today.AddDays(2)
        ), autoSave: true);

        // Caso 2: Ítem en el día límite exacto del umbral (vence en 3 días)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            _guidGenerator.Create(),
            sampleOwnerId,
            "Queso Crema",
            EstadoPantryItem.Disponible,
            today.AddDays(PantryExpirationManager.UmbralDias)
        ), autoSave: true);

        // Caso 3: Ítem fuera del umbral (vence en 10 días)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            _guidGenerator.Create(),
            sampleOwnerId,
            "Arroz Largo Fino",
            EstadoPantryItem.Disponible,
            today.AddDays(10)
        ), autoSave: true);

        // Caso 4: Ítem vencido (venció hace 3 días)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            _guidGenerator.Create(),
            sampleOwnerId,
            "Yogur Natural",
            EstadoPantryItem.Disponible,
            today.AddDays(-3)
        ), autoSave: true);

        // Caso 5: Ítem sin fecha de vencimiento
        await _pantryItemRepository.InsertAsync(new PantryItem(
            _guidGenerator.Create(),
            sampleOwnerId,
            "Sal Fina",
            EstadoPantryItem.Disponible,
            null
        ), autoSave: true);

        // Caso 6: Ítem ya consumido (con fecha en rango de alerta pero estado Consumido)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            _guidGenerator.Create(),
            sampleOwnerId,
            "Galletitas de Avena",
            EstadoPantryItem.Consumido,
            today.AddDays(1)
        ), autoSave: true);
    }
}