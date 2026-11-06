using SmartPantry.Localization;
using Volo.Abp.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace SmartPantry;

/* Inherit your application services from this class.
 */
[Authorize]
public abstract class SmartPantryAppService : ApplicationService
{
    protected SmartPantryAppService()
    {
        LocalizationResource = typeof(SmartPantryResource);
    }
}
