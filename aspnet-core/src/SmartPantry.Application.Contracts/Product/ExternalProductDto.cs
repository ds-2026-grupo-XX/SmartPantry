using System;
using System.Collections.Generic;
using System.Text;

namespace SmartPantry;
public class ExternalProductDto
{
    public string CodigoDeBarras { get; set; } = string.Empty;
    public string? NombreVisible { get; set; }
    //public string? Brand { get; set; }
    public string? Imagen { get; set; }
}