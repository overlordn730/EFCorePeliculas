using Microsoft.EntityFrameworkCore;

namespace EFCorePeliculas.Entidades.Funciones
{
    public static class Escalares
    {
        public static void RegistrarFunciones(ModelBuilder modelbuilder)
        {
            modelbuilder.HasDbFunction(() => FacturaDetallePromedio(0));
        }

        public static decimal FacturaDetallePromedio(int facturaId)
        {
            return 0;
        }
    }
}
