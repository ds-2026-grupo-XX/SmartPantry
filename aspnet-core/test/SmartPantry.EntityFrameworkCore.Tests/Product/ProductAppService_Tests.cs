using Shouldly;
using SmartPantry.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Validation;
using Xunit;
using System.Linq;

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

            var input = new CreateUpdateProductDto
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
        [Fact]
        public async Task Should_Get_Paged_List_Of_Products()
        {
            // Creamos un par de productos de prueba
            await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                CodigoDeBarras = "11111111",
                NombreVisible = "Producto A"
            });
            await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                CodigoDeBarras = "22222222",
                NombreVisible = "Producto B"
            });

            // Solicitamos la lista paginada (usando el request por defecto de ABP o PagedAndSortedResultRequestDto)
            var pagedResult = await _productAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0
            });

            pagedResult.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
            pagedResult.Items.ShouldContain(p => p.NombreVisible == "producto a"); // Validando la normalización a minúsculas
        }

        [Fact]
        public async Task Should_Update_Product()
        {
            var createdProduct = await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                CodigoDeBarras = "33333333",
                NombreVisible = "Yerba Original"
            });

            // Modificamos el producto usando el servicio
            var updateInput = new CreateUpdateProductDto
            {
                CodigoDeBarras = "33333333",
                NombreVisible = "Yerba Modificada"
            };

            await _productAppService.UpdateAsync(createdProduct.Id, updateInput);

            // Verificamos que los cambios se hayan aplicado correctamente
            var updatedProduct = await _productAppService.GetAsync(createdProduct.Id);
            updatedProduct.NombreVisible.ShouldBe("yerba modificada");
        }

        [Fact]
        public async Task Should_Delete_Product_And_Fail_On_Get()
        {
            var createdProduct = await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                CodigoDeBarras = "44444444",
                NombreVisible = "Producto a Borrar"
            });

            // Eliminamos el producto
            await _productAppService.DeleteAsync(createdProduct.Id);

            // Comprobamos que al consultar el ID eliminado arroje EntityNotFoundException (demuestra qué sucede al consultarlo luego)
            await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await _productAppService.GetAsync(createdProduct.Id);
            });
        }
        [Fact]
        public async Task Should_Register_List_Update_Get_And_Delete_Product()
        {
            // Registrar
            var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                CodigoDeBarras = "55555555",
                NombreVisible = "Fideos 500g"
            });
            created.Id.ShouldNotBe(Guid.Empty);

            // Listar
            var list = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0
            });
            list.Items.ShouldContain(p => p.Id == created.Id && p.NombreVisible == "fideos 500g");

            // Modificar
            await _productAppService.UpdateAsync(created.Id, new CreateUpdateProductDto
            {
                CodigoDeBarras = "55555556",
                NombreVisible = "Fideos Integrales"
            });

            // Consultar
            var updated = await _productAppService.GetAsync(created.Id);
            updated.CodigoDeBarras.ShouldBe("55555556");
            updated.NombreVisible.ShouldBe("fideos integrales");

            // Eliminar y comprobar
            await _productAppService.DeleteAsync(created.Id);
            await Should.ThrowAsync<EntityNotFoundException>(
                async () => await _productAppService.GetAsync(created.Id));
        }
        public class ProductAppServiceTest : SmartPantryEntityFrameworkCoreTestBase
        {
            private readonly IProductAppService _productAppService;

            public ProductAppServiceTest()
            {
                _productAppService = GetRequiredService<IProductAppService>();
            }

            [Fact]
            public async Task Shoul_Not_Create_Product_When_DTO_Not_Pass()
            {
                var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
                {
                    await _productAppService.CreateAsync(new CreateUpdateProductDto
                    {
                        NombreVisible = "",
                        CodigoDeBarras = "11223344"
                    });
                });

                exception.ValidationErrors.ShouldContain(e => e.MemberNames.Any(mem => mem == "NombreVisible"));

                exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
                {
                    await _productAppService.CreateAsync(new CreateUpdateProductDto
                    {
                        NombreVisible = "Harina 1kg",
                        CodigoDeBarras = ""
                    });
                });

                exception.ValidationErrors.ShouldContain(e => e.MemberNames.Any(mem => mem == "CodigoDeBarras"));

                exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
                {
                    await _productAppService.CreateAsync(new CreateUpdateProductDto
                    {
                        NombreVisible = "",
                        CodigoDeBarras = ""
                    });
                });

                exception.ValidationErrors.ShouldContain(e => e.MemberNames.Any(mem => mem == "CodigoDeBarras"));
                exception.ValidationErrors.ShouldContain(e => e.MemberNames.Any(mem => mem == "NombreVisible"));
            }
        }
    }
}