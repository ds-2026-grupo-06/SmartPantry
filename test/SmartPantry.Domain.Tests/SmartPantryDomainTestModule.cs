using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace SmartPantry;

[DependsOn(
    typeof(SmartPantryDomainModule),
    typeof(SmartPantryTestBaseModule)
)]
public class SmartPantryDomainTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Deshabilitar la ejecución periódica automática de Background Workers durante los tests y CI
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false;
        });
    }
}
