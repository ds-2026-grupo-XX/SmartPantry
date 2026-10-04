using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry;

public class Product : AuditedAggregateRoot<Guid>
{
    public string CodigoDeBarras { get; private set; } = string.Empty;
    public string NombreVisible { get; private set; } = string.Empty;
    public string? Imagen { get; private set; }
    public float? NutriScore { get; private set; }
    public int? Nova { get; private set; }

    // Constructor vacío requerido por ORM / ABP
    public Product() { }

    public Product(Guid id, string codDeBarra, string nombVisible, string? img = null, float? nutriScore = 0, int? nova = 1) : base(id)
    {
        ActualizarDatos(codDeBarra, nombVisible, img, nutriScore, nova);
    }

    // Método de dominio para modificar y validar de forma centralizada
    public void ActualizarDatos(string codDeBarra, string nombVisible, string? img = null, float? nutriScore = 0, int? nova = 1)
    {
        var codigoValidado = Check.NotNullOrWhiteSpace(codDeBarra, nameof(codDeBarra), maxLength: ProductConst.MaxCodDeBarraLength);
        var nombreValidado = Check.NotNullOrWhiteSpace(nombVisible, nameof(nombVisible), maxLength: ProductConst.MaxNombreVisibleLength)
                                  .ToLower();

        CodigoDeBarras = codigoValidado;
        NombreVisible = nombreValidado;
        Imagen = img;
        NutriScore = nutriScore;
        Nova = nova;
    }
}



