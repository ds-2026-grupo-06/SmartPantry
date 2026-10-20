using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.PantryItems;

public class PantryExpirationWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PantryExpirationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;

        // Configurar el intervalo de ejecución del temporizador (ej. cada 1 hora = 3600000 ms)
        // Para pruebas puedes poner un valor menor (ej. 10000 ms = 10 segundos)
        Timer.Period = 3600000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Iniciando ejecución periódica del worker de vencimientos...");

        try
        {
            // 1. Crear un Scope independiente para resolver servicios Scoped
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var expirationManager = scope.ServiceProvider.GetRequiredService<PantryExpirationManager>();
                var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

                // 2. Delimitar una Unidad de Trabajo explícita para las operaciones de escritura
                using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
                {
                    // Ejecutar la regla de negocio pasando la fecha local actual (criterio: Fecha Local)
                    await expirationManager.ProcesarVencimiento(DateTime.Now.Date);

                    await uow.CompleteAsync();
                }
            }

            Logger.LogInformation("Worker de vencimientos finalizado con éxito.");
        }
        catch (Exception ex)
        {
            // 3. Registrar el error en los logs pero NO relanzar la excepción, 
            // para garantizar que el worker siga vivo para las próximas ejecuciones.
            Logger.LogError(ex, "Ocurrió un error inesperado al procesar las advertencias de vencimiento en segundo plano.");
        }
    }
}