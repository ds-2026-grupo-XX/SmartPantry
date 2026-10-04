using System.ComponentModel.DataAnnotations;


namespace SmartPantry;

public class CreateUpdateProductDto
{
    [Required]
    [MaxLength(ProductConst.MaxCodDeBarraLength)]
    public string CodigoDeBarras { get; set; }= string.Empty;
    [Required]
    [MaxLength(ProductConst.MaxNombreVisibleLength)]
    public string NombreVisible { get; set; }= string.Empty;
    public string? Imagen { get; set; } 
    public float? NutriScore { get; set; }
    public int? Nova { get; set; }
 
}
