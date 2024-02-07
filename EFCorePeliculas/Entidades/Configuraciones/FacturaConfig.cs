using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCorePeliculas.Entidades.Configuraciones
{
    public class FacturaConfig : IEntityTypeConfiguration<Factura>
    {
        public void Configure(EntityTypeBuilder<Factura> builder)
        {
            builder.HasMany(typeof(FacturaDetalle)).WithOne();

            builder.Property(f => f.FacturaNumero)
                .HasDefaultValueSql("NEXT VALUE FOR factura.FacturaNumero");

            //builder.Property(f => f.Version).IsRowVersion();
        }
    }
}
