using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.PantryItems;

public class PantryItem : AuditedAggregateRoot<Guid>
{
    public Guid IdPropietario { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public EstadoPantryItem Estado { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? Cantidad { get; set; }
    public String? Unidad { get; set; }

    public PantryItem()
    {
    }

    public PantryItem(Guid id, Guid idPropietario, string nombreProducto, EstadoPantryItem estado, 
        DateTime? fechaVencimiento = null, int? cantidad = null, string? unidad = null)
        : base(id)
    {
        IdPropietario = idPropietario;
        NombreProducto = nombreProducto;
        Estado = estado;
        FechaVencimiento = fechaVencimiento;
        Cantidad = cantidad;
        Unidad = unidad;
    }

    public void ActualizarEstado(EstadoPantryItem estado)
    {
        Estado = estado;
    }

    public void ActualizarFechaVencimiento(DateTime? fechaVencimiento)
    {
        FechaVencimiento = fechaVencimiento;
    }
    public void ActualizarCantidad(int? cantidad)
    {
        Cantidad = cantidad;
    }
    public void ActualizarUnidad(string? unidad)
    {
        Unidad = unidad;
    }
}
