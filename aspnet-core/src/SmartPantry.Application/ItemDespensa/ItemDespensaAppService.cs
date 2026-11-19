using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace SmartPantry;

[Authorize]
public class ItemDespensaAppService : SmartPantryAppService, IItemDespensaAppService
{
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public ItemDespensaAppService(
        IRepository<ItemDespensa, Guid> itemRepository,
        IRepository<Product, Guid> productRepository)
    {
        _itemRepository = itemRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResultDto<ItemDespensaDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var userId = CurrentUser.GetId();

        var items = await _itemRepository.GetQueryableAsync();
        var products = await _productRepository.GetQueryableAsync();

        // Solo los ítems del usuario autenticado, con el nombre de su producto
        var query = from item in items
                    join product in products on item.ProductId equals product.Id
                    where item.PropietarioId == userId
                    select new { Item = item, product.NombreVisible };

        var totalCount = await AsyncExecuter.CountAsync(query);

        // Orden fijo: disponibles primero, y por fecha de vencimiento (los sin fecha al final)
        var page = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(x => x.Item.Estado)
                .ThenBy(x => x.Item.FechaVencimiento == null)
                .ThenBy(x => x.Item.FechaVencimiento)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<ItemDespensaDto>(
            totalCount,
            page.Select(x => ToDto(x.Item, x.NombreVisible)).ToList());
    }

    public async Task<ItemDespensaDto> UpdateAsync(Guid id, UpdateItemDespensaDto input)
    {
        var item = await GetOwnedItemAsync(id);

        item.ActualizarDatos(input.Cantidad, input.Unidad, input.FechaVencimiento);
        await _itemRepository.UpdateAsync(item, autoSave: true);

        return await ToDtoAsync(item);
    }

    public async Task<ItemDespensaDto> MarkAsConsumedAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);

        item.MarcarComoConsumido();
        await _itemRepository.UpdateAsync(item, autoSave: true);

        return await ToDtoAsync(item);
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);

        await _itemRepository.DeleteAsync(item, autoSave: true);
    }

    // Un ítem ajeno es indistinguible de uno inexistente: 404 en todas las operaciones
    private async Task<ItemDespensa> GetOwnedItemAsync(Guid id)
    {
        var userId = CurrentUser.GetId();

        var item = await _itemRepository.FindAsync(x => x.Id == id && x.PropietarioId == userId);
        if (item == null)
        {
            throw new EntityNotFoundException(typeof(ItemDespensa), id);
        }

        return item;
    }

    private async Task<ItemDespensaDto> ToDtoAsync(ItemDespensa item)
    {
        var product = await _productRepository.FindAsync(item.ProductId);
        return ToDto(item, product?.NombreVisible ?? string.Empty);
    }

    private static ItemDespensaDto ToDto(ItemDespensa item, string nombreProducto)
    {
        return new ItemDespensaDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            NombreProducto = nombreProducto,
            Cantidad = item.Cantidad,
            Unidad = item.Unidad,
            FechaVencimiento = item.FechaVencimiento,
            Estado = item.Estado,
            CreationTime = item.CreationTime,
            CreatorId = item.CreatorId,
            LastModificationTime = item.LastModificationTime,
            LastModifierId = item.LastModifierId
        };
    }
}