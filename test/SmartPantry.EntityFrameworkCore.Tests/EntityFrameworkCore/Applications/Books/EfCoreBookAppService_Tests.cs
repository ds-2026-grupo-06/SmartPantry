using System;
using System.Threading.Tasks;
using SmartPantry.Books;
using SmartPantry.Authors;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Applications.Books;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
}