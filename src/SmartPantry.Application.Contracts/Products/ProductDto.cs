using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products;

public class ProductDto : AuditedEntityDto<Guid>
{
    public string Barcode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string NutriScore { get; set; }
    public int NovaGroup { get; set; }
}
