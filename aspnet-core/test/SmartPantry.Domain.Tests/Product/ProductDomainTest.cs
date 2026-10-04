using System;
using Shouldly;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry
{
    public class Product_Tests
    {
        [Fact]
        public void Should_Create_Valid_Product()
        {
            var id = Guid.NewGuid();
            var codDeBarra = "77889911";
            var nombVisible = "Gaseosa N1";
            var prod = new Product(id, codDeBarra, nombVisible);

            prod.CodigoDeBarras.ShouldBe("77889911");
            prod.NombreVisible.ShouldBe("gaseosa n1");
            prod.Imagen.ShouldBeNull();
            prod.NutriScore.ShouldBe(0);
            prod.Nova.ShouldBe(1);

            var id2 = Guid.NewGuid();
            var codDeBara2 = "77889912";
            var nombVisible2 = "Pure Tomate";
            var ima = "http://img.img";
            var nutriScr = 1.2f;
            var nov = 2;

            var prod2 = new Product(id2, codDeBara2, nombVisible2, ima, nutriScr, nov);            

            prod2.CodigoDeBarras.ShouldBe("77889912");
            prod2.NombreVisible.ShouldBe("pure tomate");
            prod2.Imagen.ShouldBe("http://img.img");
            prod2.NutriScore.ShouldBe(1.2f);
            prod2.Nova.ShouldBe(2);
        }

        [Fact]
        public void Should_Not_Create_Invalid_Produc()
        {
            var id = Guid.NewGuid();

            Should.Throw<ArgumentException>(
              () => new Product(id, string.Empty, string.Empty));

            Should.Throw<ArgumentException>(
              () => new Product(id, "77889911", string.Empty));

            Should.Throw<ArgumentException>(
              () => new Product(id, string.Empty, "Gaseosa N1"));
        }
        [Fact]
        public void Should_Update_Product_Validly()
        {
            var id = Guid.NewGuid();
            var prod = new Product(id, "77889911", "Gaseosa N1");

            prod.ActualizarDatos("77889912", "Gaseosa N2 Modificada");

            prod.CodigoDeBarras.ShouldBe("77889912");
            prod.NombreVisible.ShouldBe("gaseosa n2 modificada"); // Verifica normalización
        }

        [Fact]
        public void Should_Not_Update_Product_Invalidly_And_Keep_Previous_State()
        {
            var id = Guid.NewGuid();
            var prod = new Product(id, "77889911", "Gaseosa N1");

            // Intentamos actualizar con un nombre vacío
            Should.Throw<ArgumentException>(() => prod.ActualizarDatos("77889912", string.Empty));

            // Verificamos que la entidad rechazó el cambio y mantuvo su estado anterior intacto
            prod.CodigoDeBarras.ShouldBe("77889911");
            prod.NombreVisible.ShouldBe("gaseosa n1");
        }
    }

}
