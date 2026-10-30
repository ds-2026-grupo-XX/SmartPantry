using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartPantry.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry;

public class ExpirationAlertRepository_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly IRepository<ExpirationAlert, Guid> _alertRepository;

    public ExpirationAlertRepository_Tests()
    {
        _alertRepository = GetRequiredService<IRepository<ExpirationAlert, Guid>>();
    }

    [Fact]
    public async Task Should_Reject_Duplicate_Alert_For_Same_Item_And_Type()
    {
        var itemId = Guid.NewGuid();

        await _alertRepository.InsertAsync(
            new ExpirationAlert(Guid.NewGuid(), itemId, TipoAdvertenciaVencimiento.ProximoAVencer, "Primera"),
            autoSave: true);

        // Segundo insert con el mismo ítem y tipo
        await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await _alertRepository.InsertAsync(
                new ExpirationAlert(Guid.NewGuid(), itemId, TipoAdvertenciaVencimiento.ProximoAVencer, "Duplicada"),
                autoSave: true);
        });
    }

    [Fact]
    public async Task Should_Allow_Different_Types_For_Same_Item()
    {
        var itemId = Guid.NewGuid();

        await _alertRepository.InsertAsync(
            new ExpirationAlert(Guid.NewGuid(), itemId, TipoAdvertenciaVencimiento.ProximoAVencer, "Próximo"),
            autoSave: true);
        await _alertRepository.InsertAsync(
            new ExpirationAlert(Guid.NewGuid(), itemId, TipoAdvertenciaVencimiento.Vencido, "Vencido"),
            autoSave: true);

        var alertas = await _alertRepository.GetListAsync(a => a.ItemDespensaId == itemId);
        alertas.Count.ShouldBe(2);
    }
}