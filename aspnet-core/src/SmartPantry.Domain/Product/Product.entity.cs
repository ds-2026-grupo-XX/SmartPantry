using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace SmartPantry;

public class Product : BasicAggregateRoot<Guid>
{
  [Required]
  [MaxLength(ProductConst.MaxCodDeBarraLength)]
  public string CodigoDeBarras { get; set; } = string.Empty;

  [Required]
  [MaxLength(ProductConst.MaxNombreVisibleLength)]
  public string NombreVisible { get; set; } = string.Empty;

  public string Imagen { get; set; } = string.Empty;

  public float NutriScore { get; set; } = 0;

  [MaxLength(ProductConst.MaxNovaGroup)]
  [MinLength(ProductConst.MinNovaGroup)]
  public int Nova { get; set; } = 1;
}
