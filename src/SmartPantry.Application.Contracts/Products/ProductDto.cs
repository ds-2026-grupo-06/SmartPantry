using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products;

public class ProductDto : AuditedEntityDto<Guid>
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string NutriScore { get; set; } = string.Empty;
    public int NovaGroup { get; set; } = 0;
}
