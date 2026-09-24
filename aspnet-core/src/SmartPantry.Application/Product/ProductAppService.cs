using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry
{
    public class ProductAppService : ApplicationService, IProductAppService
    {
        private readonly IRepository<Product, Guid> _productRepository;

        public ProductAppService(IRepository<Product, Guid> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var totalCount = await _productRepository.GetCountAsync();
            var products = await _productRepository.GetPagedListAsync(
                input.SkipCount,
                input.MaxResultCount,
                input.Sorting ?? nameof(Product.NombreVisible) + " asc"
            );
            var productDtos = products.Select(prod => new ProductDto
            {
                Id = prod.Id,
                NombreVisible = prod.NombreVisible,
                CodigoDeBarras = prod.CodigoDeBarras,
                Imagen = prod.Imagen,
                NutriScore = prod.NutriScore,
                Nova = prod.Nova
            }).ToList();
            return new PagedResultDto<ProductDto>(
                totalCount,
                productDtos
            );
        }
        public async Task<ProductDto> GetAsync(Guid id)
        {
            var product = await _productRepository.GetAsync(id);
            return ObjectMapper.Map<Product, ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(ProductDto product)
        {
            var prod = await _productRepository.InsertAsync(
                new Product(Guid.NewGuid(), product.CodigoDeBarras, product.NombreVisible)
                {
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
           var prod= await _productRepository.GetAsync(id);
           prod.Borrado = true;
           await _productRepository.UpdateAsync(prod);
        }
        public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto productInput)
        {
         var prod = await _productRepository.GetAsync(id);
            prod.Actualizar(
                codigoDeBarras: productInput.CodigoDeBarras,
                nombreVisible: productInput.NombreVisible,
                imagen: productInput.Imagen,
                nutriScore: productInput.NutriScore,
                nova: productInput.Nova
            );
            await _productRepository.UpdateAsync(prod);
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
    }
}