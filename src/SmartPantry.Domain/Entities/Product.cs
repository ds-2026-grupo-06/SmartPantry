using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry;

public class Product : BasicAggregateRoot<Guid>
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string NutriScore { get; set; } = string.Empty;
    public int NovaGroup { get; set; } = 0;

    public Product(Guid id, string barcode, string name, string brand, string nutriScore, int novaGroup)
        : base(id)
    {
        Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode)).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        Brand = Check.NotNullOrWhiteSpace(brand, nameof(brand)).Trim();

        NutriScore = nutriScore;
        NovaGroup = novaGroup;
    }
}