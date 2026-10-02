using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartPantry.Products.Barcode;
public class GetProductByBarcodeDto
{
    [Required]
    [StringLength(50)]
    public string Barcode { get; set; } = string.Empty;
}

