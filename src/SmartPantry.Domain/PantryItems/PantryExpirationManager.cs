using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;
using System.Linq;

namespace SmartPantry.PantryItems;

public class PantryExpirationManager : DomainService
{
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IRepository<PantryItemWarning, Guid> _warningRepository;

    // Definimos el umbral de aviso (ej: 3 días)
    private const int UmbralDias = 3;

    public PantryExpirationManager(
        IRepository<PantryItem, Guid> pantryItemRepository,
        IRepository<PantryItemWarning, Guid> warningRepository)
    {
        _pantryItemRepository = pantryItemRepository;
        _warningRepository = warningRepository;
    }

    public async Task ProcesarVencimiento(DateTime fechaActual)
    {
        var items = await _pantryItemRepository.GetListAsync();

        foreach (var item in items)
        {
            var tipoAviso = "AvisoVencimiento";

            // 1. Verificar Casos Borde: ¿Debe tener advertencia?
            // - Si no tiene fecha, o ya está consumido/descartado -> No corresponde advertencia.
            bool tieneAdvertencia = false;
            string mensaje = string.Empty;

            if (item.Estado == EstadoPantryItem.Disponible && item.FechaVencimiento.HasValue)
            {
                var diasHastaVencimiento = (item.FechaVencimiento.Value.Date - fechaActual.Date).Days;

                if (diasHastaVencimiento < 0)
                {
                    tieneAdvertencia = true;
                    mensaje = $"El producto '{item.NombreProducto}' está VENCIDO desde hace {Math.Abs(diasHastaVencimiento)} días.";
                }
                else if (diasHastaVencimiento <= UmbralDias)
                {
                    tieneAdvertencia = true;
                    mensaje = $"El producto '{item.NombreProducto}' está próximo a vencer (vence en {diasHastaVencimiento} días).";
                }
            }

            // 2. Aplicar Idempotencia (Buscar si ya existe una advertencia para este ítem)
            var existeAdvertencia = await _warningRepository.FirstOrDefaultAsync(w =>
                w.PantryItemId == item.Id && w.TipoAviso == tipoAviso);

            if (tieneAdvertencia)
            {
                if (existeAdvertencia == null)
                {
                    // No existe: Se crea por primera vez
                    var newWarning = new PantryItemWarning(
                        Guid.NewGuid(),
                        item.Id,
                        tipoAviso,
                        mensaje,
                        item.FechaVencimiento.Value
                    );
                    await _warningRepository.InsertAsync(newWarning);
                }
                else
                {
                    // Ya existe: Se actualiza (Idempotencia ante ejecuciones repetidas o cambios de fecha)
                    existeAdvertencia.ActualizarMensajeYFecha(mensaje, item.FechaVencimiento.Value);
                    await _warningRepository.UpdateAsync(existeAdvertencia);
                }
            }
            else
            {
                // Si ya no corresponde (ej. se consumió, se cambió la fecha fuera de rango, o no tiene fecha)
                if (existeAdvertencia != null && existeAdvertencia.EstaActivo)
                {
                    existeAdvertencia.Desactivar();
                    await _warningRepository.UpdateAsync(existeAdvertencia);
                }
            }
        }
    }
}