using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Entities;

public class Product : BasicAggregateRoot<Guid>
{
  public string CodigoDeBarras { get; set; } = string.Empty;
  public string NombreVisible { get; set; } = string.Empty;
  public string Imagen { get; set; } = string.Empty;
  public float NutriScore { get; set; } = 0;
  public int Nova { get; set; } = 0;
}
