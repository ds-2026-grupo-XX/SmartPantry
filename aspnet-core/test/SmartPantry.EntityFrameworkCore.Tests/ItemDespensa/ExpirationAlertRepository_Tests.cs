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
    public async Task Should_Reject_Second_Alert_For_Same_Item()
    {
        var itemId = Guid.NewGuid();

        await _alertRepository.InsertAsync(
            new ExpirationAlert(Guid.NewGuid(), itemId, EstadoVencimiento.ProximoAVencer, "Primera"),
            autoSave: true);

        // Segundo insert para el mismo ítem: el índice único (ItemDespensaId, Tipo) lo rechaza,
        // aunque el estado sea distinto
        await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await _alertRepository.InsertAsync(
                new ExpirationAlert(Guid.NewGuid(), itemId, EstadoVencimiento.Vencido, "Duplicada"),
                autoSave: true);
        });
    }

    [Fact]
    public async Task Should_Allow_One_Alert_Per_Different_Item()
    {
        var itemA = Guid.NewGuid();
        var itemB = Guid.NewGuid();

        await _alertRepository.InsertAsync(
            new ExpirationAlert(Guid.NewGuid(), itemA, EstadoVencimiento.ProximoAVencer, "Ítem A"),
            autoSave: true);
        await _alertRepository.InsertAsync(
            new ExpirationAlert(Guid.NewGuid(), itemB, EstadoVencimiento.ProximoAVencer, "Ítem B"),
            autoSave: true);

        (await _alertRepository.GetCountAsync()).ShouldBe(2);
    }
}