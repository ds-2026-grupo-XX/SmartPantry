using System;
using System.Collections.Generic;
using System.Text;

namespace SmartPantry;
public class ExternalProductDto
{
    public string CodigoDeBarras { get; set; } = string.Empty;
    public string? NombreVisible { get; set; }
    public string? Imagen { get; set; }
    public float? NutriScore { get; set; }
    public int? Nova { get; set; }
    public string? Marca { get; set; }
}