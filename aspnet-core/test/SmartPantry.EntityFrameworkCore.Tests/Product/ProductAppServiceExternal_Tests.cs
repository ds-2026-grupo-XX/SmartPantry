using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.Products;

public class ProductAppServiceExternal_Tests
{
    private readonly IExternalProductCatalogClient _externalClientMock;
    private readonly IRepository<Product, System.Guid> _repositoryMock;
    private readonly ProductAppService _productAppService;

    public ProductAppServiceExternal_Tests()
    {
        // Creamos los mocks para aislar la prueba de la base de datos y de internet
        _externalClientMock = Substitute.For<IExternalProductCatalogClient>();
        _repositoryMock = Substitute.For<IRepository<Product, System.Guid>>();

        // Instanciamos el servicio pasándole los mocks
        _productAppService = new ProductAppService(_repositoryMock, _externalClientMock);
    }

    [Fact]
    public async Task Should_Return_Product_From_External_Catalog()
    {
        // Arrange
        var barcode = "3017620422003";
        var mockDto = new ExternalProductDto
        {
            CodigoDeBarras = barcode,
            NombreVisible = "Nutella",
           // Brand = "Ferrero"
        };

        _externalClientMock.GetByBarcodeAsync(barcode).Returns(mockDto);

        var input = new BarcodeSearchInputDto { CodigoDeBarras = barcode };

        // Act
        var result = await _productAppService.GetByBarcodeAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.NombreVisible.ShouldBe("Nutella");
       // result.Brand.ShouldBe("Ferrero");
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Product_Not_Found_Externally()
    {
        // Arrange
        var barcode = "0000000000000";
        _externalClientMock.GetByBarcodeAsync(barcode).Returns((ExternalProductDto?)null);

        var input = new BarcodeSearchInputDto { CodigoDeBarras = barcode };

        // Act & Assert
        await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(input);
        });
    }
}