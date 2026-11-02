using Shouldly;
using SmartPantry.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry;

public class ExpirationProcesorService_Test : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly ExpirationProcessorService _ExpirationProcessorService;
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<ExpirationAlert, Guid> _alertRepository;
    private static readonly DateOnly Hoy = new(2026, 10, 30);

    public ExpirationProcesorService_Test()
    {
        _ExpirationProcessorService = GetRequiredService<ExpirationProcessorService>();
        _itemRepository = GetRequiredService<IRepository<ItemDespensa, Guid>>();
        _alertRepository = GetRequiredService<IRepository<ExpirationAlert, Guid>>();
    }

    private async Task<ItemDespensa> CrearItemAsync(DateOnly? fechaVencimiento)
    {
        var item = new ItemDespensa(
            Guid.NewGuid(),
            propietarioId: Guid.NewGuid(),
            productId: Guid.NewGuid(),
            cantidad: 1,
            unidad: "unidades",
            fechaVencimiento: fechaVencimiento
        );

        await _itemRepository.InsertAsync(item, autoSave: true);
        return item;
    }

    [Fact]
    public async Task Should_Create_Expiration_Warning_When_Item_Is_Close_To_Expire()
    {
        var item = await CrearItemAsync(Hoy);

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var advertencias = await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id);
        advertencias.Count.ShouldBe(1);

        var adv = advertencias[0];
        adv.Tipo.ShouldBe(TipoAdvertencia.Vencimiento);
        adv.Estado.ShouldBe(EstadoVencimiento.ProximoAVencer);
        adv.Activa.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Create_Warning_On_Threshold_Limit_Day()
    {
        // Día límite inclusivo: vence exactamente en hoy + umbral
        var item = await CrearItemAsync(Hoy.AddDays(ExpirationConst.UmbralDiasAviso));

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var advertencias = await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id);
        advertencias.Count.ShouldBe(1);
        advertencias[0].Estado.ShouldBe(EstadoVencimiento.ProximoAVencer);
        advertencias[0].Activa.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Create_Warning_When_Item_Is_Outside_Threshold()
    {
        // Un día después del límite: todavía no corresponde avisar
        var item = await CrearItemAsync(Hoy.AddDays(ExpirationConst.UmbralDiasAviso + 1));

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var advertencias = await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id);
        advertencias.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Create_Expired_Warning_Only_When_Date_Is_Before_Today()
    {
        var venceHoy = await CrearItemAsync(Hoy);
        var venceAyer = await CrearItemAsync(Hoy.AddDays(-1));

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var alertaHoy = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == venceHoy.Id)).Single();
        alertaHoy.Estado.ShouldBe(EstadoVencimiento.ProximoAVencer);

        var alertaAyer = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == venceAyer.Id)).Single();
        alertaAyer.Estado.ShouldBe(EstadoVencimiento.Vencido);
        alertaAyer.Activa.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Create_Warning_When_Item_Has_No_Expiration_Date()
    {
        var item = await CrearItemAsync(fechaVencimiento: null);

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var advertencias = await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id);
        advertencias.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Deactivate_Warning_When_Item_Is_Consumed()
    {
        var item = await CrearItemAsync(Hoy.AddDays(1));

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var alerta = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();
        alerta.Activa.ShouldBeTrue();

        // El usuario consume el ítem: la advertencia deja de corresponder
        item.MarcarComoConsumido();
        await _itemRepository.UpdateAsync(item, autoSave: true);

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var alertaDespues = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();
        alertaDespues.Id.ShouldBe(alerta.Id);
        alertaDespues.Activa.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Be_Idempotent_When_Run_Multiple_Times()
    {
        var item = await CrearItemAsync(Hoy.AddDays(-1));

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);
        var primera = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var advertencias = await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id);
        advertencias.Count.ShouldBe(1);
        advertencias[0].Id.ShouldBe(primera.Id);
        advertencias[0].Estado.ShouldBe(EstadoVencimiento.Vencido);
    }

    [Fact]
    public async Task Should_Update_Deactivate_And_Reactivate_Same_Warning_When_Date_Changes()
    {
        var item = await CrearItemAsync(Hoy.AddDays(1));

        // Se crea la advertencia
        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);
        var original = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();
        original.Activa.ShouldBeTrue();

        // Cambia la fecha pero sigue dentro del umbral: se actualiza el mensaje
        var nuevaFecha = Hoy.AddDays(2);
        item.ActualizarFechaVencimiento(nuevaFecha);
        await _itemRepository.UpdateAsync(item, autoSave: true);
        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var actualizada = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();
        actualizada.Id.ShouldBe(original.Id);
        actualizada.Activa.ShouldBeTrue();
        actualizada.Mensaje.ShouldContain(nuevaFecha.ToString("yyyy-MM-dd"));

        // La fecha sale del umbral: se desactiva, no se borra
        item.ActualizarFechaVencimiento(Hoy.AddDays(30));
        await _itemRepository.UpdateAsync(item, autoSave: true);
        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var desactivada = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();
        desactivada.Id.ShouldBe(original.Id);
        desactivada.Activa.ShouldBeFalse();

        // La fecha vuelve a entrar en el umbral: se reactiva la misma advertencia
        item.ActualizarFechaVencimiento(Hoy.AddDays(1));
        await _itemRepository.UpdateAsync(item, autoSave: true);
        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);

        var reactivada = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();
        reactivada.Id.ShouldBe(original.Id);
        reactivada.Activa.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Keep_Single_Warning_And_Change_State_When_Item_Goes_From_Soon_To_Expired()
    {
        var item = await CrearItemAsync(Hoy.AddDays(1));

        await _ExpirationProcessorService.ProcesarVencimientosAsync(Hoy);
        var original = (await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id)).Single();
        original.Estado.ShouldBe(EstadoVencimiento.ProximoAVencer);

        // Pasan los días: el mismo ítem ahora está vencido
        var pasadoManana = Hoy.AddDays(2);
        await _ExpirationProcessorService.ProcesarVencimientosAsync(pasadoManana);

        var alertas = await _alertRepository.GetListAsync(a => a.ItemDespensaId == item.Id);
        alertas.Count.ShouldBe(1);
        alertas[0].Id.ShouldBe(original.Id);
        alertas[0].Estado.ShouldBe(EstadoVencimiento.Vencido);
        alertas[0].Activa.ShouldBeTrue();
    }
}