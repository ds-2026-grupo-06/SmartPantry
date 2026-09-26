using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using SmartPantry.Products;

namespace SmartPantry;

[Mapper]
public partial class ProductToProductDtoMapper : MapperBase<Product, ProductDto>
{
   public override partial ProductDto Map(Product source);

    public override partial void Map(Product source, ProductDto destination);
}
[Mapper]
public partial class CreateUpdateProductDtoToProductMapper : MapperBase<CreateUpdateProductDto, Product>
{
    public override partial Product Map(CreateUpdateProductDto source);

    public override partial void Map(CreateUpdateProductDto source, Product destination);
}

