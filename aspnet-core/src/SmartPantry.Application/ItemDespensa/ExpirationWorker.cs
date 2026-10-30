using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;


namespace SmartPantry;

public class ExpirationWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ExpirationWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 60 * 60 * 1000; // cada 1 hora
        Timer.RunOnStart = true;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        // El servicio se resuelve en el alcance de cada ejecución:
        // el worker no guarda repositorios ni servicios scoped.
        var service = workerContext.ServiceProvider.GetRequiredService<ExpirationProcessorService>();

        // Criterio de fecha: UTC
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        await service.ProcesarVencimientosAsync(hoy);
    }
}