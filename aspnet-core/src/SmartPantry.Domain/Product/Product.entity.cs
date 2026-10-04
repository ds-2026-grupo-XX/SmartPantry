using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry;

public class Product : BasicAggregateRoot<Guid>
{
  public string CodigoDeBarras { get; set; } = string.Empty;
  public string NombreVisible { get; set; } = string.Empty;
  public string Imagen { get; set; } = string.Empty;
  public float NutriScore { get; set; } = 0;
  public int Nova { get; set; } = 1;

  protected Product() { }

  public Product(Guid id, string codDeBarra, string nombVisible) : base(id)
  {
    CodigoDeBarras = Check.NotNullOrWhiteSpace(codDeBarra, nameof(codDeBarra), maxLength: ProductConst.MaxCodDeBarraLength);
    NombreVisible = Check.NotNullOrWhiteSpace(nombVisible, nameof(nombVisible), maxLength: ProductConst.MaxNombreVisibleLength);

    NombreVisible = nombVisible.ToLower();
  }
}
