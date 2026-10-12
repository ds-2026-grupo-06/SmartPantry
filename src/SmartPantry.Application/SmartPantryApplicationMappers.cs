using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using SmartPantry.Products;

namespace SmartPantry;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ProductToProductDtoMapper : MapperBase<Product, SmartPantry.Products.ProductDto>
{
    public override partial SmartPantry.Products.ProductDto Map(Product source);

    public override partial void Map(Product source, SmartPantry.Products.ProductDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class CreateUpdateProductDtoToProductMapper : MapperBase<CreateUpdateProductDto, Product>
{
    public override partial Product Map(CreateUpdateProductDto source);

    public override partial void Map(CreateUpdateProductDto source, Product destination);
}

