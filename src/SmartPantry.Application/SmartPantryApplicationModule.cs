using Microsoft.Extensions.DependencyInjection;
using SmartPantry.PantryItems;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;

namespace SmartPantry;

[DependsOn(
    typeof(SmartPantryDomainModule),
    typeof(SmartPantryApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpMapperlyModule)
    )]
public class SmartPantryApplicationModule : AbpModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // Registrar el worker de vencimientos al iniciar la aplicación (respeta AbpBackgroundWorkerOptions.IsEnabled en pruebas)
        AsyncHelper.RunSync(async () =>
        {
            await context.AddBackgroundWorkerAsync<PantryExpirationWorker>();
        });
    }
}