using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Products;

public class CreateUpdateProductDto
{
    [Required]
    [StringLength(50)]
    public string Barcode { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(5)]
    public string NutriScore { get; set; } = string.Empty;

    [Required]
    public int NovaGroup { get; set; } = 0;
}
