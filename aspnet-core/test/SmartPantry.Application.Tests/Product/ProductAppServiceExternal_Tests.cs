using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.Products;

public class ProductAppServiceExternal_Tests
{
    private const string ValidBarcode = "3017620422003";
    private const string UnknownBarcode = "0000000000000";

    private readonly IExternalProductCatalogClient _externalClientMock;
    private readonly IRepository<Product, Guid> _repositoryMock;
    private readonly ProductAppService _productAppService;

    public ProductAppServiceExternal_Tests()
    {
        
        _externalClientMock = Substitute.For<IExternalProductCatalogClient>();
        _repositoryMock = Substitute.For<IRepository<Product, Guid>>();

        _productAppService = new ProductAppService(_repositoryMock, _externalClientMock);
    }

    private static BarcodeSearchInputDto Input(string barcode) =>
        new() { CodigoDeBarras = barcode };

    // Producto encontrado / no encontrado 

    [Fact]
    public async Task Should_Return_Product_From_External_Catalog()
    {
        // Arrange
        _externalClientMock.GetByBarcodeAsync(ValidBarcode).Returns(new ExternalProductDto
        {
            CodigoDeBarras = ValidBarcode,
            Encontrado = true,
            NombreVisible = "Nutella",
            Marca = "Ferrero",
            Imagen = "https://images.example/nutella.jpg"
        });

        // Act
        var result = await _productAppService.GetByBarcodeAsync(Input(ValidBarcode));

        // Assert
        result.ShouldNotBeNull();
        result.Encontrado.ShouldBeTrue();
        result.NombreVisible.ShouldBe("Nutella");
        result.Marca.ShouldBe("Ferrero");
        result.Imagen.ShouldBe("https://images.example/nutella.jpg");
        await _externalClientMock.Received(1).GetByBarcodeAsync(ValidBarcode);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Product_Not_Found_Externally()
    {
        // Arrange
        _externalClientMock.GetByBarcodeAsync(UnknownBarcode).Returns(new ExternalProductDto
        {
            CodigoDeBarras = UnknownBarcode,
            Encontrado = false
        });

        // Act & Assert
        await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(Input(UnknownBarcode));
        });
    }

    // Fallas del proveedor

    [Fact]
    public async Task Should_ThrowException_When_RateLimitExceeded()
    {
        // Arrange
        const string message = "Se ha superado el limite de solicitudes a la API Externa. Intente mas tarde";
        _externalClientMock.GetByBarcodeAsync(ValidBarcode)
            .ThrowsAsync(new UserFriendlyException(message));

        // Act & Assert
        var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(Input(ValidBarcode));
        });
        ex.Message.ShouldBe(message);
    }

    [Fact]
    public async Task Should_ThrowException_When_ProviderUnavailable()
    {
        // Arrange
        const string message = "Error en la API Externa. Intente mas tarde";
        _externalClientMock.GetByBarcodeAsync(ValidBarcode)
            .ThrowsAsync(new UserFriendlyException(message));

        // Act & Assert
        var ex = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _productAppService.GetByBarcodeAsync(Input(ValidBarcode));
        });
        ex.Message.ShouldBe(message);
    }

    // Datos ausentes

    [Fact]
    public async Task Should_Keep_Name_Unreported_When_Provider_Does_Not_Return_It()
    {
        _externalClientMock.GetByBarcodeAsync(ValidBarcode).Returns(new ExternalProductDto
        {
            CodigoDeBarras = ValidBarcode,
            Encontrado = true,
            NombreVisible = null,
            Marca = "Ferrero",
            Imagen = "https://images.example/p.jpg"
        });

        var result = await _productAppService.GetByBarcodeAsync(Input(ValidBarcode));

        result.Encontrado.ShouldBeTrue();
        result.NombreVisible.ShouldBeNull();
        result.Marca.ShouldBe("Ferrero");
        result.Imagen.ShouldBe("https://images.example/p.jpg");
    }

    [Fact]
    public async Task Should_Keep_Brand_Unreported_When_Provider_Does_Not_Return_It()
    {
        _externalClientMock.GetByBarcodeAsync(ValidBarcode).Returns(new ExternalProductDto
        {
            CodigoDeBarras = ValidBarcode,
            Encontrado = true,
            NombreVisible = "Producto Sin Marca",
            Marca = null,
            Imagen = "https://images.example/p.jpg"
        });

        var result = await _productAppService.GetByBarcodeAsync(Input(ValidBarcode));

        result.Encontrado.ShouldBeTrue();
        result.NombreVisible.ShouldBe("Producto Sin Marca");
        result.Marca.ShouldBeNull();
        result.Imagen.ShouldBe("https://images.example/p.jpg");
    }

    [Fact]
    public async Task Should_Keep_Image_Unreported_When_Provider_Does_Not_Return_It()
    {
        _externalClientMock.GetByBarcodeAsync(ValidBarcode).Returns(new ExternalProductDto
        {
            CodigoDeBarras = ValidBarcode,
            Encontrado = true,
            NombreVisible = "Producto Sin Imagen",
            Marca = "Ferrero",
            Imagen = null
        });

        var result = await _productAppService.GetByBarcodeAsync(Input(ValidBarcode));

        result.Encontrado.ShouldBeTrue();
        result.NombreVisible.ShouldBe("Producto Sin Imagen");
        result.Marca.ShouldBe("Ferrero");
        result.Imagen.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Keep_Name_Brand_And_Image_Unreported_When_All_Are_Missing()
    {
        _externalClientMock.GetByBarcodeAsync(ValidBarcode).Returns(new ExternalProductDto
        {
            CodigoDeBarras = ValidBarcode,
            Encontrado = true,
            NombreVisible = null,
            Marca = null,
            Imagen = null
        });

        var result = await _productAppService.GetByBarcodeAsync(Input(ValidBarcode));

        result.ShouldNotBeNull();
        result.Encontrado.ShouldBeTrue();
        result.NombreVisible.ShouldBeNull();
        result.Marca.ShouldBeNull();
        result.Imagen.ShouldBeNull();
    }
}