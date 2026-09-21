using Shouldly;
using SmartPantry.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace SmartPantry.Products
{
    public class ProductAppService_Tests : SmartPantryEntityFrameworkCoreTestBase
    {
        private readonly IProductAppService _productAppService;

        public ProductAppService_Tests()
        {
           _productAppService = GetRequiredService<IProductAppService>();
        }

        [Fact]
        public async Task Should_Get_Product_By_Id_And_Handle_Not_Found()
        {
            
            var input = new ProductDto
            {
                CodigoDeBarras = "12345678",
                NombreVisible = "Prueba",
                Imagen = "prueba",
                NutriScore = 1,
                Nova = 2
            };

            var createdProduct = await _productAppService.CreateAsync(input);

            // (ID Existente): Obtenemos el producto por su ID real
            var retrievedProduct = await _productAppService.GetAsync(createdProduct.Id);

            // Verificamos que traiga los datos correctos
            retrievedProduct.ShouldNotBeNull();
            retrievedProduct.Id.ShouldBe(createdProduct.Id);
            retrievedProduct.CodigoDeBarras.ShouldBe("12345678");
            retrievedProduct.NombreVisible.ShouldBe("prueba");

            //(ID Inexistente): Probamos pasar un ID aleatorio que no exista
            var nonExistentId = Guid.NewGuid();

            await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await _productAppService.GetAsync(nonExistentId);
            });
        }
    }
}