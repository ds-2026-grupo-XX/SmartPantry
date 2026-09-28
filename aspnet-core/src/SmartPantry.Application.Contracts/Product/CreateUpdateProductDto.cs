using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartPantry;

public class CreateUpdateProductDto
{
    [Required]
    [MaxLength(ProductConst.MaxCodDeBarraLength)]
    public string? CodigoDeBarras { get; set; } 
    [Required]
    [MaxLength(ProductConst.MaxNombreVisibleLength)]
    public string? NombreVisible { get; set; } 
    public string? Imagen { get; set; } 
    public float? NutriScore { get; set; }
    public int? Nova { get; set; }
    public bool Borrado { get; set; }
}
