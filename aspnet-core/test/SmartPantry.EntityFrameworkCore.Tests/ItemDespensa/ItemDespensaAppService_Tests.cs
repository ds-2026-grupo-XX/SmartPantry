using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;
using Volo.Abp.Validation;

namespace SmartPantry;

public class ItemDespensaAppService_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    private static readonly Guid UsuarioA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UsuarioB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly IItemDespensaAppService _service;
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    public ItemDespensaAppService_Tests()
    {
        _service = GetRequiredService<IItemDespensaAppService>();
        _itemRepository = GetRequiredService<IRepository<ItemDespensa, Guid>>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    // Simula que el usuario autenticado es userId mientras dure el using
    private IDisposable ComoUsuario(Guid userId) =>
        _principalAccessor.Change(new Claim(AbpClaimTypes.UserId, userId.ToString()));

    private Task<Product> CrearProductoAsync(string codigo, string nombre) =>
        _productRepository.InsertAsync(new Product(Guid.NewGuid(), codigo, nombre), autoSave: true);

    private Task<ItemDespensa> CrearItemAsync(
        Guid propietarioId, Guid productId, int cantidad = 1,
        string? unidad = "unidades", DateOnly? fechaVencimiento = null) =>
        _itemRepository.InsertAsync(
            new ItemDespensa(Guid.NewGuid(), propietarioId, productId, cantidad, unidad, fechaVencimiento),
            autoSave: true);

    [Fact]
    public async Task Should_List_Only_Own_Items_With_Product_Name()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var propio = await CrearItemAsync(UsuarioA, producto.Id);
        await CrearItemAsync(UsuarioB, producto.Id);

        using (ComoUsuario(UsuarioA))
        {
            var result = await _service.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 10 });

            result.TotalCount.ShouldBe(1);
            result.Items.Single().Id.ShouldBe(propio.Id);
            result.Items.Single().NombreProducto.ShouldBe("arroz 1kg"); // Product normaliza el nombre a minúsculas
        }
    }

    [Fact]
    public async Task Should_Update_Own_Item()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var item = await CrearItemAsync(UsuarioA, producto.Id, cantidad: 1, unidad: "unidades");
        var nuevaFecha = new DateOnly(2026, 12, 31);

        using (ComoUsuario(UsuarioA))
        {
            var result = await _service.UpdateAsync(item.Id, new UpdateItemDespensaDto
            {
                Cantidad = 5,
                Unidad = "kg",
                FechaVencimiento = nuevaFecha
            });

            result.Cantidad.ShouldBe(5);
            result.Unidad.ShouldBe("kg");
            result.FechaVencimiento.ShouldBe(nuevaFecha);
        }

        var guardado = await _itemRepository.GetAsync(item.Id);
        guardado.Cantidad.ShouldBe(5);
        guardado.Unidad.ShouldBe("kg");
        guardado.FechaVencimiento.ShouldBe(nuevaFecha);
    }

    [Fact]
    public async Task Should_Not_Update_Item_Of_Another_User()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var ajeno = await CrearItemAsync(UsuarioB, producto.Id, cantidad: 1, unidad: "unidades");

        using (ComoUsuario(UsuarioA))
        {
            // Un ítem ajeno se comporta como inexistente: 404
            await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await _service.UpdateAsync(ajeno.Id, new UpdateItemDespensaDto { Cantidad = 99, Unidad = "kg" });
            });
        }

        var sinCambios = await _itemRepository.GetAsync(ajeno.Id);
        sinCambios.Cantidad.ShouldBe(1);
        sinCambios.Unidad.ShouldBe("unidades");
    }
    [Fact]
    public async Task Should_Return_Empty_List_When_User_Has_No_Items()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        await CrearItemAsync(UsuarioB, producto.Id);

        using (ComoUsuario(UsuarioA))
        {
            var result = await _service.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 10 });

            result.TotalCount.ShouldBe(0);
            result.Items.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task Should_Mark_Own_Item_As_Consumed_And_Keep_It()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var item = await CrearItemAsync(UsuarioA, producto.Id);

        using (ComoUsuario(UsuarioA))
        {
            var result = await _service.MarkAsConsumedAsync(item.Id);
            result.Estado.ShouldBe(EstadoItemDespensa.Consumido);
        }

        // Regla de negocio 6: el consumido no se elimina, se conserva como historial
        var guardado = await _itemRepository.GetAsync(item.Id);
        guardado.Estado.ShouldBe(EstadoItemDespensa.Consumido);
    }

    [Fact]
    public async Task Should_Not_Mark_Item_Of_Another_User_As_Consumed()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var ajeno = await CrearItemAsync(UsuarioB, producto.Id);

        using (ComoUsuario(UsuarioA))
        {
            await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await _service.MarkAsConsumedAsync(ajeno.Id);
            });
        }

        var sinCambios = await _itemRepository.GetAsync(ajeno.Id);
        sinCambios.Estado.ShouldBe(EstadoItemDespensa.Disponible);
    }

    [Fact]
    public async Task Should_Delete_Own_Item()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var item = await CrearItemAsync(UsuarioA, producto.Id);

        using (ComoUsuario(UsuarioA))
        {
            await _service.DeleteAsync(item.Id);
        }

        (await _itemRepository.FindAsync(item.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task Should_Not_Delete_Item_Of_Another_User()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var ajeno = await CrearItemAsync(UsuarioB, producto.Id);

        using (ComoUsuario(UsuarioA))
        {
            await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await _service.DeleteAsync(ajeno.Id);
            });
        }

        (await _itemRepository.FindAsync(ajeno.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Reject_Negative_Quantity_And_Too_Long_Unit()
    {
        var producto = await CrearProductoAsync("11112222", "Arroz 1kg");
        var item = await CrearItemAsync(UsuarioA, producto.Id, cantidad: 2, unidad: "unidades");

        using (ComoUsuario(UsuarioA))
        {
            var errorCantidad = await Should.ThrowAsync<AbpValidationException>(async () =>
            {
                await _service.UpdateAsync(item.Id, new UpdateItemDespensaDto { Cantidad = -1 });
            });
            errorCantidad.ValidationErrors.ShouldContain(e => e.MemberNames.Any(m => m == "Cantidad"));

            var errorUnidad = await Should.ThrowAsync<AbpValidationException>(async () =>
            {
                await _service.UpdateAsync(item.Id, new UpdateItemDespensaDto
                {
                    Cantidad = 1,
                    Unidad = new string('a', ItemDespensaConst.MaxUnidadLength + 1)
                });
            });
            errorUnidad.ValidationErrors.ShouldContain(e => e.MemberNames.Any(m => m == "Unidad"));
        }

        // Ninguna de las dos actualizaciones inválidas modificó el ítem
        var sinCambios = await _itemRepository.GetAsync(item.Id);
        sinCambios.Cantidad.ShouldBe(2);
        sinCambios.Unidad.ShouldBe("unidades");
    }
}