using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry;

public class ExpirationAlert : AuditedAggregateRoot<Guid>
{
    public Guid ItemDespensaId { get; private set; }
    public TipoAdvertencia Tipo { get; private set; }
    public EstadoVencimiento Estado { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;
    public bool Activa { get; private set; }

    // Constructor vacío requerido por ORM / ABP
    protected ExpirationAlert() { }

    // Constructor de dominio
    public ExpirationAlert(
        Guid id,
        Guid itemDespensaId,
        EstadoVencimiento estado,
        string mensaje) : base(id)
    {
        ItemDespensaId = itemDespensaId;
        Tipo = TipoAdvertencia.Vencimiento;
        Estado = estado;
        Mensaje = mensaje;
        Activa = true;
    }

    public void ActualizarEstado(EstadoVencimiento nuevoEstado, string nuevoMensaje)
    {
        Estado = nuevoEstado;
        Mensaje = nuevoMensaje;
        Activa = true;
    }

    public void Desactivar()
    {
        Activa = false;
    }

    
}


