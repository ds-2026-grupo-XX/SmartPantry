using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry;

public class ProductAppService :
    CrudAppService<Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>,
    IProductAppService
{
    private readonly IExternalProductCatalogClient _externalClient;
    public ProductAppService(IRepository<Product, Guid> repository, IExternalProductCatalogClient externalClient) : base(repository)
    {
        _externalClient = externalClient;
    }

    public override async Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        await CheckCreatePolicyAsync();

        var product = new Product(
            GuidGenerator.Create(),
            input.CodigoDeBarras,
            input.NombreVisible,
            input.Imagen,
            input.NutriScore,
            input.Nova);

        await Repository.InsertAsync(product, autoSave: true);
        return await MapToGetOutputDtoAsync(product);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input)
    {
        await CheckUpdatePolicyAsync();

        var product = await Repository.GetAsync(id);
        product.ActualizarDatos(
            input.CodigoDeBarras,
            input.NombreVisible,
            input.Imagen,
            input.NutriScore,
            input.Nova);

        await Repository.UpdateAsync(product, autoSave: true);
        return await MapToGetOutputDtoAsync(product);
    }
    public async Task<ExternalProductDto> GetByBarcodeAsync(BarcodeSearchInputDto input)
    {
        // Consultamos al cliente externo usando la abstracción (sin conocer HTTP ni JSONs)
        var externalProduct = await _externalClient.GetByBarcodeAsync(input.CodigoDeBarras);

        if (externalProduct == null)
        {
            throw new UserFriendlyException("El producto no fue encontrado en el catálogo externo.");
        }

        return externalProduct;
    }
}