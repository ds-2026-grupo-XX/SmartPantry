using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry;

public class ItemDespensa : AuditedAggregateRoot<Guid>
{
    public Guid PropietarioId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Cantidad { get; private set; }
    public string? Unidad { get; private set; } 
    public DateOnly? FechaVencimiento { get; private set; }
    public EstadoItemDespensa Estado { get; private set; }

    // Constructor vacío requerido por ORM / ABP
    public ItemDespensa() { }
    public ItemDespensa(
    Guid id,
    Guid propietarioId,
    Guid productId,
    int cantidad,
    string? unidad = null,
    DateOnly? fechaVencimiento = null) : base(id)
    {
        if (cantidad < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidad), cantidad, "La cantidad no puede ser negativa.");
        }

        PropietarioId = propietarioId;
        ProductId = productId;
        Cantidad = cantidad;
        Unidad = Check.Length(unidad, nameof(unidad), ItemDespensaConst.MaxUnidadLength);
        FechaVencimiento = fechaVencimiento;
        Estado = EstadoItemDespensa.Disponible;
    }
    public void MarcarComoConsumido()
    {
        Estado = EstadoItemDespensa.Consumido;
    }

    public void ActualizarFechaVencimiento(DateOnly? nuevaFecha)
    {
        FechaVencimiento = nuevaFecha;
    }



}
