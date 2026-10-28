using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SmartPantry.EntityFrameworkCore;
using SmartPantry.Products;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry;

public class ExpirationProcesorService_Test : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly ExpirationProcessorService _ExpirationProcessorService;
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<ExpirationAlert, Guid> _alertRepository;

    public ExpirationProcesorService_Test()
    {
        _ExpirationProcessorService = GetRequiredService<ExpirationProcessorService>();
        _itemRepository = GetRequiredService<IRepository<ItemDespensa, Guid>>();
        _alertRepository = GetRequiredService<IRepository<ExpirationAlert, Guid>>();
    }

    [Fact]
    public async Task Should_Create_Expiration_Warning_When_Item_Is_Close_To_Expire()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var itemId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var item = new ItemDespensa(
            itemId,
            propietarioId: Guid.NewGuid(),
            productId: productId,
            cantidad: 2,
            unidad: "unidades",
            fechaVencimiento: hoy
        );

        await _itemRepository.InsertAsync(item, autoSave: true);

        await _ExpirationProcessorService.ProcesarVencimientosAsync();

        var advertencias = await _alertRepository.GetListAsync(a => a.ItemDespensaId == itemId);
        advertencias.Count.ShouldBe(1);

        var adv = advertencias[0];
        adv.Tipo.ShouldBe(TipoAdvertenciaVencimiento.ProximoAVencer);
        adv.Activa.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Be_Idempotent_When_Run_Multiple_Times()
    {
        var ayer = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var itemId = Guid.NewGuid();

        var item = new ItemDespensa(
            itemId,
            propietarioId: Guid.NewGuid(),
            productId: Guid.NewGuid(),
            cantidad: 1,
            unidad: "kg",
            fechaVencimiento: ayer
        );

        await _itemRepository.InsertAsync(item, autoSave: true);

        // Prueba de idempotencia: ejecutar dos veces seguidas
        await _ExpirationProcessorService.ProcesarVencimientosAsync();
        await _ExpirationProcessorService.ProcesarVencimientosAsync();

        var advertencias = await _alertRepository.GetListAsync(a => a.ItemDespensaId == itemId);
        advertencias.Count.ShouldBe(1);
        advertencias[0].Tipo.ShouldBe(TipoAdvertenciaVencimiento.Vencido);
    }
}