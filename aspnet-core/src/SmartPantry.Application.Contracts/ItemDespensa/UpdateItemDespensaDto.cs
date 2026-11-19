using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry;

public class UpdateItemDespensaDto
{
    [Range(0, int.MaxValue)]
    public int Cantidad { get; set; }

    [MaxLength(ItemDespensaConst.MaxUnidadLength)]
    public string? Unidad { get; set; }

    public DateOnly? FechaVencimiento { get; set; }
}