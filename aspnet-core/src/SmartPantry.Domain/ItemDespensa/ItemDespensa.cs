using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry;

public class ItemDespensa : AuditedAggregateRoot<Guid>
{
    public Guid PropietarioId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Cantidad { get; private set; }
    public string? Unidad { get; private set; } //  lo debemos poner en clases?
    public DateOnly? FechaVencimiento { get; private set; }
    public EstadoItemDespensa Estado { get; private set; }

    // Constructor vacío requerido por ORM / ABP
    public ItemDespensa() { }
    public ItemDespensa(
        Guid id,
        Guid propietarioId,
        Guid productId,
        int cantidad,
        string unidad,
        DateOnly? fechaVencimiento = null) : base(id)
    {
        PropietarioId = propietarioId;
        ProductId = productId;
        Cantidad = cantidad;
        Unidad = unidad;
        FechaVencimiento = fechaVencimiento;
        Estado = EstadoItemDespensa.Disponible; // Nace disponible por defecto
    }
    public void MarcarComoConsumido()
    {
        Estado = EstadoItemDespensa.Consumido;
    }

    public void ActualizarFechaVencimiento(DateOnly? nuevaFecha)
    {
        FechaVencimiento = nuevaFecha;
    }


public enum EstadoItemDespensa
{
    Disponible = 1,
    Consumido = 2
}
}
