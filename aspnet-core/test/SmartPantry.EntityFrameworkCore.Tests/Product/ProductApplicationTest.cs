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
  public class ProductAppService_Tests : SmartPantryEntityFrameworkCoreTestBase

  {
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductAppService_Tests()
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
         await _productAppService.CreateAsync(new ProductDto
         {
           NombreVisible = "",
           CodigoDeBarras = "11223344"
         });
       });

      exception.ValidationErrors.ShouldContain(e => e.MemberNames.Any(mem => mem == "NombreVisible"));

      exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
       {
         await _productAppService.CreateAsync(new ProductDto
         {
           NombreVisible = "Harina 1kg",
           CodigoDeBarras = ""
         });
       });

      exception.ValidationErrors.ShouldContain(e => e.MemberNames.Any(mem => mem == "CodigoDeBarras"));

      exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
       {
         await _productAppService.CreateAsync(new ProductDto
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
