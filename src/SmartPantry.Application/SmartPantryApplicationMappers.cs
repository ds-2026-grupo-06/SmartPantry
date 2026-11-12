using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using SmartPantry.Products;
using SmartPantry.PantryItems;

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

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PantryItemToPantryItemDtoMapper : MapperBase<PantryItem, PantryItemDto>
{
    public override partial PantryItemDto Map(PantryItem source);

    public override partial void Map(PantryItem source, PantryItemDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class CreateUpdatePantryItemDtoToPantryItemMapper : MapperBase<CreateUpdatePantryItemDto, PantryItem>
{
    public override partial PantryItem Map(CreateUpdatePantryItemDto source);

    public override partial void Map(CreateUpdatePantryItemDto source, PantryItem destination);
}

