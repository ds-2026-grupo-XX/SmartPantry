using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry;

[Authorize]
public class ProductAppService :
    CrudAppService<Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>,
    IProductAppService
{
    private readonly IExternalProductCatalogClient _externalClient;
    // private readonly ExpirationProcessorService _expirationProcessorService; // Comentado (prueba de expiración)

    public ProductAppService(
        IRepository<Product, Guid> repository,
        IExternalProductCatalogClient externalClient/*,
        ExpirationProcessorService expirationProcessorService*/) : base(repository)
    {
        _externalClient = externalClient;
        // _expirationProcessorService = expirationProcessorService;
    }

    public override async Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        await CheckCreatePolicyAsync();

        var product = new Product(
            GuidGenerator.Create(),
            codDeBarra: input.CodigoDeBarras,
            nombVisible: input.NombreVisible,
            marca: input.Marca,
            img: input.Imagen,
            nutriScore: input.NutriScore,
            nova: input.Nova);

        await Repository.InsertAsync(product, autoSave: true);
        return await MapToGetOutputDtoAsync(product);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input)
    {
        await CheckUpdatePolicyAsync();

        var product = await Repository.GetAsync(id);

        product.ActualizarDatos(
            codDeBarra: input.CodigoDeBarras,
            nombVisible: input.NombreVisible,
            marca: input.Marca,
            img: input.Imagen,
            nutriScore: input.NutriScore,
            nova: input.Nova);

        await Repository.UpdateAsync(product, autoSave: true);
        return await MapToGetOutputDtoAsync(product);
    }

    public async Task<ExternalProductDto> GetByBarcodeAsync(BarcodeSearchInputDto input)
    {
        var externalProduct = await _externalClient.GetByBarcodeAsync(input.CodigoDeBarras);

        if (externalProduct == null || !externalProduct.Encontrado)
        {
            throw new UserFriendlyException("El producto no fue encontrado en el catálogo externo.");
        }

        return externalProduct;
    }

    /*
    // Endpoint temporal comentado para dejar el servicio limpio
    public async Task TestExpirationAsync()
    {
        await _expirationProcessorService.ProcesarVencimientosAsync(DateOnly.FromDateTime(DateTime.Today));
    }
    */
}