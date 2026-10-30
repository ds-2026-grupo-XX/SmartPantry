using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using static SmartPantry.ItemDespensa;

namespace SmartPantry;

public class ExpirationProcessorService : ITransientDependency
{
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<ExpirationAlert, Guid> _alertRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ExpirationProcessorService(
        IRepository<ItemDespensa, Guid> itemRepository,
        IRepository<ExpirationAlert, Guid> alertRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _itemRepository = itemRepository;
        _alertRepository = alertRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task ProcesarVencimientosAsync(DateOnly hoy)
    {
        var fechaUmbral = hoy.AddDays(3);

        var items = await _itemRepository.GetListAsync(
            x => x.Estado == EstadoItemDespensa.Disponible && x.FechaVencimiento != null
        );

        foreach (var item in items)
        {
            var fechaVenc = item.FechaVencimiento!.Value;
            TipoAdvertenciaVencimiento? tipoDetectado = null;
            string mensaje = string.Empty;

            if (fechaVenc < hoy)
            {
                tipoDetectado = TipoAdvertenciaVencimiento.Vencido;
                mensaje = $"El producto ha vencido el {fechaVenc:yyyy-MM-dd}.";
            }
            else if (fechaVenc <= fechaUmbral)
            {
                tipoDetectado = TipoAdvertenciaVencimiento.ProximoAVencer;
                mensaje = $"El producto está próximo a vencer el {fechaVenc:yyyy-MM-dd}.";
            }

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
            {
                var advertenciaExistente = await _alertRepository.FirstOrDefaultAsync(
                    a => a.ItemDespensaId == item.Id && a.Tipo == tipoDetectado
                );

                if (tipoDetectado != null)
                {
                    if (advertenciaExistente == null)
                    {
                        var nuevaAdvertencia = new ExpirationAlert(
                            Guid.NewGuid(),
                            item.Id,
                            tipoDetectado.Value,
                            mensaje
                        );
                        await _alertRepository.InsertAsync(nuevaAdvertencia);
                    }
                    else
                    {
                        advertenciaExistente.ActualizarMensaje(mensaje);
                        await _alertRepository.UpdateAsync(advertenciaExistente);
                    }
                }

                var advertenciasActivasDelItem = await _alertRepository.GetListAsync(
                    a => a.ItemDespensaId == item.Id && a.Activa
                );

                foreach (var adv in advertenciasActivasDelItem)
                {
                    if (adv.Tipo != tipoDetectado)
                    {
                        adv.Desactivar();
                        await _alertRepository.UpdateAsync(adv);
                    }
                }

                await uow.CompleteAsync();
            }
        }

        // Alertas activas de ítems que ya no corresponden (consumidos o sin fecha)
        var idsVigentes = items.Select(i => i.Id).ToHashSet();
        var alertasActivas = await _alertRepository.GetListAsync(a => a.Activa);
        var sinCorresponder = alertasActivas
            .Where(a => !idsVigentes.Contains(a.ItemDespensaId))
            .ToList();

        if (sinCorresponder.Count > 0)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
            {
                foreach (var alerta in sinCorresponder)
                {
                    alerta.Desactivar();
                    await _alertRepository.UpdateAsync(alerta);
                }

                await uow.CompleteAsync();
            }
        }
    }
}