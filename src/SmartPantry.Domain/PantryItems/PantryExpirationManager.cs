using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;
using System.Linq;

namespace SmartPantry.PantryItems;

/// <summary>
/// Servicio de dominio encargado de evaluar el vencimiento de los ítems de la despensa (RF-15).
/// 
/// Criterio de fecha:
/// - Se utiliza FECHA LOCAL (DateTime.Now), tomando únicamente la fecha calendario (.Date) 
///   tanto en la fecha de referencia como en la fecha de vencimiento del ítem.
/// 
/// Regla de umbral (UmbralDias = 3):
/// - Ítem no disponible (Consumido / Descartado) o sin FechaVencimiento: NO corresponde advertencia (se desactiva si existía).
/// - Días hasta vencimiento &lt; 0: VENCIDO -&gt; corresponde advertencia activa.
/// - Días hasta vencimiento entre 0 y 3 (inclusive el día límite = 3): PRÓXIMO A VENCER -&gt; corresponde advertencia activa.
/// - Días hasta vencimiento &gt; 3: FUERA DEL UMBRAL -&gt; NO corresponde advertencia (se desactiva si existía).
/// </summary>
public class PantryExpirationManager : DomainService
{
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IRepository<PantryItemWarning, Guid> _warningRepository;

    // Umbral de aviso en días calendario (incluye el día límite = 3 días)
    public const int UmbralDias = 3;

    // Clave lógica estable de tipo de advertencia por ítem
    public const string TipoAvisoVencimiento = "AvisoVencimiento";

    public PantryExpirationManager(
        IRepository<PantryItem, Guid> pantryItemRepository,
        IRepository<PantryItemWarning, Guid> warningRepository)
    {
        _pantryItemRepository = pantryItemRepository;
        _warningRepository = warningRepository;
    }

    public async Task ProcesarVencimiento(DateTime fechaActual)
    {
        // Criterio explícito: Fecha Local truncada al día calendario (.Date)
        var fechaReferencia = fechaActual.Date;
        var items = await _pantryItemRepository.GetListAsync();

        foreach (var item in items)
        {
            var tipoAviso = TipoAvisoVencimiento;

            // 1. Verificar casos de negocio y borde: ¿Debe tener advertencia activa?
            // - Si no tiene fecha, o ya está consumido/descartado -> No corresponde advertencia.
            bool tieneAdvertencia = false;
            string mensaje = string.Empty;

            if (item.Estado == EstadoPantryItem.Disponible && item.FechaVencimiento.HasValue)
            {
                var diasHastaVencimiento = (item.FechaVencimiento.Value.Date - fechaReferencia).Days;

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

            // 2. Aplicar Idempotencia buscando por la clave lógica estable (PantryItemId, TipoAviso)
            var existeAdvertencia = await _warningRepository.FirstOrDefaultAsync(w =>
                w.PantryItemId == item.Id && w.TipoAviso == tipoAviso);

            if (tieneAdvertencia)
            {
                if (existeAdvertencia == null)
                {
                    // No existe: Se crea por primera vez y se confirma en BD (autoSave: true)
                    var newWarning = new PantryItemWarning(
                        GuidGenerator.Create(),
                        item.Id,
                        tipoAviso,
                        mensaje,
                        item.FechaVencimiento!.Value.Date
                    );
                    await _warningRepository.InsertAsync(newWarning, autoSave: true);
                }
                else
                {
                    // Ya existe: Se actualiza mensaje, fecha y se asegura que quede activa (reactivación si estaba inactiva)
                    existeAdvertencia.ActualizarMensajeYFecha(mensaje, item.FechaVencimiento!.Value.Date);
                    await _warningRepository.UpdateAsync(existeAdvertencia, autoSave: true);
                }
            }
            else
            {
                // Si ya no corresponde (ej. se consumió, se cambió la fecha fuera del umbral, o se quitó la fecha)
                if (existeAdvertencia != null && existeAdvertencia.EstaActivo)
                {
                    existeAdvertencia.Desactivar();
                    await _warningRepository.UpdateAsync(existeAdvertencia, autoSave: true);
                }
            }
        }
    }
}