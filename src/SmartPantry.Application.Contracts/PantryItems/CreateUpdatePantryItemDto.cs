using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.PantryItems;
public class CreateUpdatePantryItemDto
{
    [Required]
    public Guid IdPropietario { get; set; }
    [Required]
    public string NombreProducto { get; set; } = string.Empty;
    [Required]
    public EstadoPantryItem Estado { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? Cantidad { get; set; }
    public String? Unidad { get; set; }
}
