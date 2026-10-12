using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Products;

public class Product : AuditedAggregateRoot<Guid>
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string NutriScore { get; set; } = string.Empty;
    public int NovaGroup { get; set; }

    // Constructor sin parámetros (lo necesita el DataSeed y EF Core)
    public Product()
    {
    }

    // Constructor con ID y validaciones de negocio
    public Product(Guid id, string barcode, string name, string brand, string nutriScore, int novaGroup)
        : base(id)
    {
        Barcode = barcode?.Trim() ?? string.Empty;
        UpdateDetails(name, brand, nutriScore, novaGroup);
    }

    public void UpdateDetails(string name, string brand, string nutriScore, int novaGroup)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.NotNullOrWhiteSpace(brand, nameof(brand));

        if (novaGroup < 1 || novaGroup > 4)
        {
            throw new BusinessException("SmartPantry:NovaGroupInvalid")
                .WithData("Value", novaGroup);
        }

        Name = name.Trim();
        Brand = brand.Trim();
        NutriScore = nutriScore?.Trim().ToUpperInvariant() ?? string.Empty;
        NovaGroup = novaGroup;
    }
}