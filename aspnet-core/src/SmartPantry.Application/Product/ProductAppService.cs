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

    public ProductAppService(
        IRepository<Product, Guid> repository,
        IExternalProductCatalogClient externalClient) : base(repository)
    {
        _externalClient = externalClient;
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
}