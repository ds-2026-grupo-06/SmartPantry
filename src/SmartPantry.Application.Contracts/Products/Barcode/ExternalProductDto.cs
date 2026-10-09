using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products.Barcode;
public class ExternalProductDto : EntityDto<Guid>
{
    public string Barcode { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Quantity { get; set; }
    public string ImageUrl { get; set; }
    public string Nutrients { get;set; }

}

