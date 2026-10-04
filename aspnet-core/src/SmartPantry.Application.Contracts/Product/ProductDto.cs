using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry;

public class ProductDto
{
  public Guid Id { get; set; }
  [Required]
  [MaxLength(ProductConst.MaxCodDeBarraLength)]
  public string CodigoDeBarras { get; set; } = string.Empty;
  [Required]
  [MaxLength(ProductConst.MaxNombreVisibleLength)]

  public string NombreVisible { get; set; } = string.Empty;
  public string Imagen { get; set; } = string.Empty;
  public float NutriScore { get; set; } = 0;
  public int Nova { get; set; } = 1;
}
