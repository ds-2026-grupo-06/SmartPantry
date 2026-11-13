using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.PantryItems;

public class CreateUpdatePantryItemDto : IValidatableObject
{
    public Guid IdPropietario { get; set; }

    [Required]
    [StringLength(128)]
    public string NombreProducto { get; set; } = string.Empty;

    [Required]
    public EstadoPantryItem Estado { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    [Range(0, int.MaxValue)]
    public int? Cantidad { get; set; }

    [StringLength(32)]
    public string? Unidad { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaVencimiento.HasValue && FechaVencimiento.Value == default)
        {
            yield return new ValidationResult(
                "La fecha de vencimiento no es válida.",
                new[] { nameof(FechaVencimiento) }
            );
        }
    }
}
