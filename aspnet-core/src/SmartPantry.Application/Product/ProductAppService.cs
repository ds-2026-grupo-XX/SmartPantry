using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry;

public class ProductAppService :
    CrudAppService<Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository) : base(repository)
    {
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
}