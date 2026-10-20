using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.PantryItems;

public class PantryItemWarning : AuditedAggregateRoot<Guid>
{
    public Guid PantryItemId { get; set; }
    public string TipoAviso { get; set; } // Ej: "ExpiringSoon" o "Expired"
    public string Mensaje { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime FechaVencimiento { get; set; }


    public PantryItemWarning(Guid id, Guid pantryItemId, string tipoAviso, string mensaje, DateTime fechaVencimiento)
        : base(id)
    {
        PantryItemId = pantryItemId;
        TipoAviso = tipoAviso;
        Mensaje = mensaje;
        FechaVencimiento = fechaVencimiento;
        EstaActivo = true;
    }

    public void ActualizarMensajeYFecha(string mensaje, DateTime fechaVencimiento)
    {
        Mensaje = mensaje;
        FechaVencimiento = fechaVencimiento;
        EstaActivo  = true;
    }

    public void Desactivar()
    {
        EstaActivo = false;
    }
}

