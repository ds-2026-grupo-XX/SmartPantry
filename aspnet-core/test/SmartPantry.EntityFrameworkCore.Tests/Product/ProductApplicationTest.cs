using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry
{
    public class ProductRepository_Tests : SmartPantryEntityFrameworkCoreTestBase
    {
        private readonly IRepository<Product, Guid> _productRepository;

        public ProductRepository_Tests()
        {
            _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        }

        [Fact]
        public async Task Should_Get_List_Of_Products()
        {
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "11223344", "Arroz 1kg"));
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "22334411", "Azucar 1kg"));
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "44223311", "Yerba 1kg"));

            var result = await _productRepository.GetListAsync();

            result.Count.ShouldBe(3);
        }

        [Fact]
        public async Task Should_Create_A_New_Product()
        {
            var id = Guid.NewGuid();
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "11223344", "Arroz 1kg"));
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "22334411", "Azucar 1kg"));
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "44223311", "Yerba 1kg"));
            await _productRepository.InsertAsync(new Product(id, "44223333", "Harina 1kg"));

            var result = await _productRepository.GetAsync(id);

            result.NombreVisible.ShouldBe("harina 1kg");
        }

        [Fact]
        public async Task Should_Delete_Product()
        {
            var id = Guid.NewGuid();
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "11223344", "Arroz 1kg"));
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "22334411", "Azucar 1kg"));
            await _productRepository.InsertAsync(new Product(Guid.NewGuid(), "44223311", "Yerba 1kg"));
            await _productRepository.InsertAsync(new Product(id, "44223333", "Harina 1kg"));

            await _productRepository.DeleteAsync(id);

            var deleteProd = await _productRepository.FindAsync(id);
            var result = await _productRepository.GetListAsync();

            deleteProd.ShouldBeNull();
            result.Count.ShouldBe(3);
        }
    }

    
}
