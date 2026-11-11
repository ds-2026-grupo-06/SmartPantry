using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using SmartPantry.PantryItems;

namespace SmartPantry.PantryItems;
public class PantryItemDto : AuditedEntityDto<Guid>
{
    public Guid IdPropietario { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public EstadoPantryItem Estado { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? Cantidad { get; set; }
    public String? Unidad { get; set; }
}
