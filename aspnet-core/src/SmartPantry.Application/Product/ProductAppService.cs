using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry;

public class ProductAppService : ApplicationService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductAppService(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> GetListAsync()
    {
        var products = await _productRepository.GetListAsync();
        return products
            .Select(prod => new ProductDto
            {
                Id = prod.Id,
                NombreVisible = prod.NombreVisible,
                CodigoDeBarras = prod.CodigoDeBarras,
                Imagen = prod.Imagen,
                NutriScore = prod.NutriScore,
                Nova = prod.Nova
            }).ToList();
    }

    public async Task<ProductDto> CreateAsync(ProductDto product)
    {
        var prod = await _productRepository.InsertAsync(
            new Product
            {
                NombreVisible = product.NombreVisible,
                CodigoDeBarras = product.CodigoDeBarras,
                Imagen = product.Imagen,
                NutriScore = product.NutriScore,
                Nova = product.Nova
            }
        );

        return new ProductDto
        {
            Id = prod.Id,
            NombreVisible = prod.NombreVisible,
            CodigoDeBarras = prod.CodigoDeBarras,
            Imagen = prod.Imagen,
            NutriScore = prod.NutriScore,
            Nova = prod.Nova
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        await _productRepository.DeleteAsync(id);
    }

}
