using System;

namespace SmartPantry;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string NutriScore { get; set; } = string.Empty;
    public int NovaGroup { get; set; } = 0;
}

