using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry;

public class Product : AuditedAggregateRoot<Guid>
{
    public string CodigoDeBarras { get; set; } 
    public string NombreVisible { get; set; } 
    public string? Imagen { get; set; } 
    public float NutriScore { get; set; } 
    public int Nova { get; set; } 
    public bool Borrado { get; set; } 

}


