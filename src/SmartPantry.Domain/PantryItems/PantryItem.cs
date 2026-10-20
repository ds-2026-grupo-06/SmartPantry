using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.PantryItems;

public class PantryItem : AuditedAggregateRoot<Guid>
{
    public Guid IdPropietario { get; set; }
    public string NombreProducto { get; set; }
    public EstadoPantryItem Estado { get; set; }
    public DateTime? FechaVencimiento { get; set; }

    public PantryItem(Guid id,Guid idPropietario, string nombreProducto, EstadoPantryItem estado, DateTime? fechaVencimiento = null)
        : base(id)
    {
        IdPropietario = idPropietario;
        NombreProducto = nombreProducto;
        Estado = estado;
        FechaVencimiento = fechaVencimiento;
    }

    public void ActualizarEstado(EstadoPantryItem estado)
    {
        Estado = estado;
    }

    public void ActualizarFechaVencimiento(DateTime? fechaVencimiento)
    {
        FechaVencimiento = fechaVencimiento;
    }
}
