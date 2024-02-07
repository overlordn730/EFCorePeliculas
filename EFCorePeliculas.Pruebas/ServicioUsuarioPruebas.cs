using EFCorePeliculas.Servicios;

namespace EFCorePeliculas.Pruebas
{
    [TestClass]
    public class ServicioUsuarioPruebas
    {
        [TestMethod]
        public void ObtenerUsuarioId_NoTraeValorVacioONulo()
        {
            //Preparacion
            var serviciousuario = new ServicioUsuario();

            //Prueba
            var resultado = serviciousuario.ObtenerUsuarioId();

            //Verificacion
            Assert.IsNotNull(resultado);
            Assert.AreNotEqual("", resultado);
        }
    }
}