using System.ComponentModel.DataAnnotations;

namespace SmartPantry;

public class BarcodeSearchInputDto
{
    [Required]
    [StringLength(ProductConst.MaxCodDeBarraLength, MinimumLength = 1)]
    public string CodigoDeBarras { get; set; } = string.Empty;
}