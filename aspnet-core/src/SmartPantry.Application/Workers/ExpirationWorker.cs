using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace SmartPantry;

public class ExpirationWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ExpirationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
    {
        // Intervalo de ejecución (ejemplo: cada 24 horas = 86400000 ms)
        Timer.Period = 86400000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        // Resolvemos el servicio usando el scope factory del contexto
        using var scope = workerContext.ServiceProvider.CreateScope();
        var expirationProcessorService = scope.ServiceProvider.GetRequiredService<ExpirationProcessorService>();

        await expirationProcessorService.ProcesarVencimientosAsync(DateOnly.FromDateTime(DateTime.Today));
    }
}