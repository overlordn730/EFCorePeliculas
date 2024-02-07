using EFCorePeliculas.Entidades;
using EFCorePeliculas.Entidades.Funciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace EFCorePeliculas.Controllers
{
    [ApiController]
    [Route("api/facturas")]
    public class FacturasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<FacturasController> _logger;

        public FacturasController(ApplicationDbContext context, ILogger<FacturasController> logger)
        {
            this.context = context;
            this._logger = logger;
        }

        [HttpGet("get-totales/{id:int}")]
        public async Task<ActionResult<IEnumerable<FacturaDetalle>>> GetTotalesOrdered(int id)
        {
            var resultado = await context.FacturaDetalles.Where(f => f.FacturaId == id).OrderByDescending(f => f.Total).ToListAsync();
            return Ok(resultado);
        }

        [HttpGet("funciones-escalares")]
        public async Task<ActionResult> DetallesTotalSubTotalAllFactura()
        {
            var facturas = await context.Facturas.Select(f => new
            {
                Id = f.Id,
                ImporteTotal = context.FacturaDetalleSuma(f.Id),
                Promedio = Escalares.FacturaDetallePromedio(f.Id)
            })
                .OrderByDescending(f => context.FacturaDetalleSuma(f.Id))
                .ToListAsync();

            return Ok(facturas);
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            using var transaccion = await context.Database.BeginTransactionAsync();
            try
            {
                var factura = new Factura()
                {
                    FechaCreacion = DateTime.Now
                };

                context.Add(factura);
                await context.SaveChangesAsync();

                throw new ApplicationException("Esto es una prueba");

                var facturaDetalle = new List<FacturaDetalle>()
                        {
                            new FacturaDetalle()
                            {
                                Producto = "Producto A",
                                Precio = 123,
                                FacturaId = factura.Id
                            },
                            new FacturaDetalle()
                            {
                                Producto = "Producto B",
                                Precio = 456,
                                FacturaId = factura.Id
                            }
                        };

                context.AddRange(facturaDetalle);
                await context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return Ok("todo bien");
            }
            catch (Exception ex)
            {
                // Manejar excepción
                return BadRequest("Hubo un error");
            }
        }

        /// <summary>
        /// Método para mostrar todas las facturas
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-facturas")]
        public async Task<ActionResult> getFacturas()
        {
            var facturas = await context.Facturas.ToListAsync();

            return Ok(facturas);
        }

        [HttpGet("control-concurrencia-fila")]
        public async Task<ActionResult> ManejoConcurrenciaFila()
        {
            var facturaId = 2;

            try
            {
                var factura = await context.Facturas.AsTracking().FirstOrDefaultAsync(f => f.Id == facturaId);
                factura.FechaCreacion = DateTime.Now.AddDays(-10);

                await context.Database.ExecuteSqlInterpolatedAsync(@$"update Facturas set FechaCreacion = GetDate() where Id = {facturaId}");

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var facturaActual = await context.Facturas.AsNoTracking().FirstOrDefaultAsync(f => f.Id == facturaId);
                foreach (var propiedad in entry.Metadata.GetProperties())
                {
                    var valorIntentado = entry.Property(propiedad.Name).CurrentValue;
                    var valorDBActual = context.Entry(facturaActual).Property(propiedad.Name).CurrentValue;
                    var valorAnterior = entry.Property(propiedad.Name).OriginalValue;

                    if (valorIntentado.ToString() == valorDBActual.ToString())
                    {
                        // No se realizó la actualización
                        continue;
                    }

                    _logger.LogInformation($"--- Propiedad {propiedad.Name} ---");
                    _logger.LogInformation($"Valor intentado: {valorIntentado}");
                    _logger.LogInformation($"Valor en la base de datos: {valorDBActual}");
                    _logger.LogInformation($"Valor anterior: {valorAnterior}");

                    //Hacer algo...
                }
                return BadRequest("Conflicto de concurrencia al actualizar el registro");
            }
        }

        [HttpGet("concurrencia-fila")]
        public async Task<ActionResult> ConcurrenciaFila()
        {
            var facturaId = 2;

            var factura = await context.Facturas.AsTracking().FirstOrDefaultAsync(f => f.Id == facturaId);
            factura.FechaCreacion = DateTime.Now;

            await context.Database.ExecuteSqlInterpolatedAsync(@$"update Facturas set FechaCreacion = GetDate() where Id = {facturaId}");

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("obtener-factura/{id:int}")]
        public async Task<ActionResult<Factura>> ObtenerFacturaPorId(int id)
        {
            var factura = await context.Facturas.FirstOrDefaultAsync(f => f.Id == id);

            if (factura is null)
            {
                return NotFound();
            }

            return Ok(factura);
        }
    }
}
