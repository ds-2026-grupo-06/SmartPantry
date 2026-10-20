using SmartPantry.PantryItems;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Domains;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCorePantryExpirationManager_Tests : PantryExpirationManager_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
}
