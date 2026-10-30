using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry;

public class ExpirationAlert : AuditedAggregateRoot<Guid>
{
    public Guid ItemDespensaId { get; private set; }
    public TipoAdvertenciaVencimiento Tipo { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;
    public bool Activa { get; private set; }

    // Constructor vacío requerido por ORM / ABP
    protected ExpirationAlert() { }

    // Constructor de dominio
    public ExpirationAlert(
        Guid id,
        Guid itemDespensaId,
        TipoAdvertenciaVencimiento tipo,
        string mensaje) : base(id)
    {
        ItemDespensaId = itemDespensaId;
        Tipo = tipo;
        Mensaje = mensaje;
        Activa = true; 
    }

    public void ActualizarMensaje(string nuevoMensaje)
    {
        Mensaje = nuevoMensaje;
        Activa = true; 
    }

    public void Desactivar()
    {
        Activa = false;
    }

    public void Activar()
    {
        Activa = true;
    }
}


