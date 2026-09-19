using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry;

public class Product : BasicAggregateRoot<Guid>
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string NutriScore { get; set; } = string.Empty;
    public int NovaGroup { get; set; } = 0;
}