using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.PantryItems;

public class PantryDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;

    public PantryDataSeedContributor(IRepository<PantryItem, Guid> pantryItemRepository)
    {
        _pantryItemRepository = pantryItemRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 1. Evitar duplicar datos si ya se ejecutó el seeding previamente
        if (await _pantryItemRepository.GetCountAsync() > 0)
        {
            return;
        }

        // Usamos un ID de propietario fijo para simular los datos iniciales
        var sampleOwnerId = Guid.NewGuid();
        var today = DateTime.Now.Date;

        // 2. Insertar los casos de prueba requeridos por el TP
        // Ítem próximo a vencer (dentro del umbral de 3 días, ej: en 2 días)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            Guid.NewGuid(),
            sampleOwnerId,
            "Leche Descremada",
            EstadoPantryItem.Disponible,
            today.AddDays(2)
        ));

        // Ítem vencido (ej: venció hace 3 días)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            Guid.NewGuid(),
            sampleOwnerId,
            "Yogur Natural",
            EstadoPantryItem.Disponible,
            today.AddDays(-3)
        ));

        // Ítem sin fecha de vencimiento
        await _pantryItemRepository.InsertAsync(new PantryItem(
            Guid.NewGuid(),
            sampleOwnerId,
            "Sal Fina",
            EstadoPantryItem.Disponible,
            null
        ));

        // Ítem ya consumido (con fecha en rango de alerta pero estado consumido)
        await _pantryItemRepository.InsertAsync(new PantryItem(
            Guid.NewGuid(),
            sampleOwnerId,
            "Galletitas de Avena",
            EstadoPantryItem.Consumido,
            today.AddDays(1)
        ));
    }
}