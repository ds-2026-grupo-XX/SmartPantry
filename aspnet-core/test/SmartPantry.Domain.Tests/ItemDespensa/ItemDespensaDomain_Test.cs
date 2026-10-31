using System;
using Shouldly;
using Xunit;

namespace SmartPantry;

public class ItemDespensa_Tests
{
    [Fact]
    // item sin unidad y fecha de vencimiento 
    public void Should_Create_Item_Available_With_Optional_Unit_And_Date()
    {
        var item = new ItemDespensa(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2);

        item.Estado.ShouldBe(EstadoItemDespensa.Disponible);
        item.Cantidad.ShouldBe(2);
        item.Unidad.ShouldBeNull();
        item.FechaVencimiento.ShouldBeNull();
    }

    [Fact]
    // item con cantidad negativa
    public void Should_Not_Create_Item_With_Negative_Quantity()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => new ItemDespensa(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -1));
    }

    [Fact]
    // item con largo de unidad mayor al permitido y con largo de unidad dentro del permitido
    public void Should_Accept_Unit_With_Max_Length_And_Reject_Longer()
    {
        var unidadMaxima = new string('a', ItemDespensaConst.MaxUnidadLength);
        var item = new ItemDespensa(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, unidadMaxima);
        item.Unidad.ShouldBe(unidadMaxima);

        var unidadLarga = new string('a', ItemDespensaConst.MaxUnidadLength + 1);
        Should.Throw<ArgumentException>(
            () => new ItemDespensa(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, unidadLarga));
    }

    [Fact]
    //item consumido
    public void Should_Mark_Item_As_Consumed()
    {
        var item = new ItemDespensa(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);

        item.MarcarComoConsumido();

        item.Estado.ShouldBe(EstadoItemDespensa.Consumido);
    }
}