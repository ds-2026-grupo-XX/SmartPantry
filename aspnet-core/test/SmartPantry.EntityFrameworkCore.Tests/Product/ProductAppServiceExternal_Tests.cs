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
            Marca = "Ferrero"
        };

        _externalClientMock.GetByBarcodeAsync(barcode).Returns(mockDto);

        var input = new BarcodeSearchInputDto { CodigoDeBarras = barcode };

        // Act
        var result = await _productAppService.GetByBarcodeAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.NombreVisible.ShouldBe("Nutella");
       result.Marca.ShouldBe("Ferrero");
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
    [Fact]
    public async Task Should_ThrowException_When_RateLimitExceeded()
    {
        // Arrange
        var barcode = "3017620422003";
        _externalClientMock.GetByBarcodeAsync(barcode).Returns(Task.FromException<ExternalProductDto?>(new UserFriendlyException("Se ha superado el limite de solicitudes a la API Externa. Intente mas tarde")
        ));
        var input = new BarcodeSearchInputDto { CodigoDeBarras = barcode };
        // Act & Assert
        await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(input);
        });
    }
    [Fact]
    public async Task Should_ThrowException_when_APIError()
    {
        // Arrange
        var barcode = "3017620422003";
        _externalClientMock.GetByBarcodeAsync(barcode).Returns(Task.FromException<ExternalProductDto?>(new UserFriendlyException("Error en la API Externa. Intente mas tarde")
        ));
        var input = new BarcodeSearchInputDto { CodigoDeBarras = barcode };
        // Act & Assert
        await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(input);
        });
    }
    [Fact]
    public async Task Should_HandleProduct_When_BrandIsMissing()
    {
        // Arrange
        var barcode = "3017620422003";
        var externalDto = new ExternalProductDto
        {
            CodigoDeBarras = barcode,
            NombreVisible = "Producto Sin Marca",
            Marca = null 
        };

        _externalClientMock.GetByBarcodeAsync(barcode).Returns(externalDto);
        var input = new BarcodeSearchInputDto { CodigoDeBarras = barcode };

        // Act
        var result = await _productAppService.GetByBarcodeAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Marca.ShouldBeNull();
    }
}