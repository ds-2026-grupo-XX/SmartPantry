using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry;

public class ItemDespensaDto : AuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string? Unidad { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public EstadoItemDespensa Estado { get; set; }
}