using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartPantry.EntityFrameworkCore;
using SmartPantry.Products;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace SmartPantry;

[DependsOn(
    typeof(SmartPantryApplicationModule),
    typeof(SmartPantryEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(AbpMapperlyModule),
    typeof(SmartPantryDomainTestModule) 
)]
public class SmartPantryApplicationTestModule : AbpModule
{
    private AbpUnitTestSqliteDatabase? _database;

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpSqliteOptions>(x => x.BusyTimeout = null);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);

        // Registramos un Mock de IExternalProductCatalogClient para que las pruebas no salgan a Internet
        // Consulta de productos por código de barras, simulando la interacción con un cliente externo.
        var externalCatalogClientMock = Substitute.For<IExternalProductCatalogClient>();
        context.Services.AddSingleton(externalCatalogClientMock);
    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _database = new AbpUnitTestSqliteDatabase();
        _database.CreateTables(
            new SmartPantryDbContext(new DbContextOptionsBuilder<SmartPantryDbContext>().UseSqlite(_database.ConnectionString).Options));

        services.Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = _database.ConnectionString;
        });

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.UseSqlite();
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _database?.Dispose();
    }
}
