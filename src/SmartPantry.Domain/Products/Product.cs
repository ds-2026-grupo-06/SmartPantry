using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Products;

public class Product : AuditedAggregateRoot<Guid>
{
    public string Barcode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string NutriScore { get; set; }
    public int NovaGroup { get; set; }
}