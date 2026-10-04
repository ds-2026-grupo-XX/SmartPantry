using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry;

public class ProductDto: AuditedEntityDto<Guid>
{

  public string CodigoDeBarras { get; set; } = string.Empty;


  public string NombreVisible { get; set; } = string.Empty;
  public string? Imagen { get; set; } = null;
  public float? NutriScore { get; set; } = 0;
  public int? Nova { get; set; } = 1;
   
}
