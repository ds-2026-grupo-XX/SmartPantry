using System.ComponentModel.DataAnnotations;

namespace SmartPantry;

public class BarcodeSearchInputDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string CodigoDeBarras { get; set; } = string.Empty;
}