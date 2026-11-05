using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace SmartPantry.DbMigrator;

/// <summary>
/// Datos locales de ejemplo para probar el worker de vencimientos (TP08).
/// El propietario es un Guid fijo: todavía no hay seguridad, y no existe API para elegirlo.
/// Es idempotente: si ya existen ítems de este propietario, no hace nada.
/// </summary>
public class ItemDespensaDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly Guid PropietarioDemoId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IGuidGenerator _guidGenerator;

    public ItemDespensaDataSeedContributor(
        IRepository<Product, Guid> productRepository,
        IRepository<ItemDespensa, Guid> itemRepository,
        IGuidGenerator guidGenerator)
    {
        _productRepository = productRepository;
        _itemRepository = itemRepository;
        _guidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (await _itemRepository.AnyAsync(i => i.PropietarioId == PropietarioDemoId))
        {
            return;
        }

        var producto = await _productRepository.FindAsync(p => p.CodigoDeBarras == "3017620422003");
        if (producto == null)
        {
            producto = await _productRepository.InsertAsync(
                new Product(_guidGenerator.Create(), "3017620422003", "Nutella", "Ferrero"),
                autoSave: true);
        }

        // Fechas relativas a hoy (UTC), el mismo criterio que usa el worker
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        await CrearItemAsync(producto.Id, "unidades", hoy.AddDays(1));    // próximo a vencer
        await CrearItemAsync(producto.Id, "unidades", hoy.AddDays(-2));   // vencido
        await CrearItemAsync(producto.Id, "kg", hoy.AddDays(10));         // fuera del umbral
        await CrearItemAsync(producto.Id, "unidades", null);              // sin fecha

        var consumido = await CrearItemAsync(producto.Id, "unidades", hoy.AddDays(1));
        consumido.MarcarComoConsumido();                                  // consumido: no avisa
        await _itemRepository.UpdateAsync(consumido, autoSave: true);
    }

    private Task<ItemDespensa> CrearItemAsync(Guid productId, string unidad, DateOnly? fechaVencimiento)
    {
        return _itemRepository.InsertAsync(
            new ItemDespensa(_guidGenerator.Create(), PropietarioDemoId, productId, 1, unidad, fechaVencimiento),
            autoSave: true);
    }
}