using GestionCompras.Data;
using GestionCompras.Models;
using GestionCompras.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionCompras.Business.Services
{
    /// <summary>
    /// Servicio para gestionar Órdenes de Compra
    /// </summary>
    public class OrdenCompraService : IOrdenCompraService
    {
        private readonly GestionComprasDbContext _context;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<OrdenCompraService> _logger;

        public OrdenCompraService(GestionComprasDbContext context, 
            IAuditoriaService auditoriaService, 
            ILogger<OrdenCompraService> logger)
        {
            _context = context;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva orden de compra
        /// </summary>
        public async Task<OrdenCompra> CrearOrdenAsync(OrdenCompra orden, List<DetalleOrdenCompra> detalles)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Generar número de orden
                    orden.NumeroOrden = GenerarNumeroOrden();
                    orden.FechaOrden = DateTime.Now;
                    orden.Estado = "Pendiente";
                    orden.Total = 0;

                    _context.OrdenesCompra.Add(orden);
                    await _context.SaveChangesAsync();

                    // Agregar detalles
                    if (detalles != null && detalles.Count > 0)
                    {
                        decimal totalOrden = 0;
                        foreach (var detalle in detalles)
                        {
                            detalle.OrdenID = orden.OrdenID;
                            detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                            totalOrden += detalle.Subtotal;
                            _context.DetallesOrdenesCompra.Add(detalle);
                        }
                        orden.Total = totalOrden;
                        _context.OrdenesCompra.Update(orden);
                    }

                    await _context.SaveChangesAsync();

                    // Registrar en auditoría
                    await _auditoriaService.RegistrarOperacionAsync(
                        usuarioId: 1, 
                        nombreUsuario: orden.CreadoPor,
                        modulo: "OrdenesCompra",
                        tipoOperacion: "INSERT",
                        idRegistro: orden.OrdenID,
                        nombreRegistro: orden.NumeroOrden,
                        descripcion: $"Orden de compra creada por {orden.CreadoPor}",
                        ip: "127.0.0.1"
                    );

                    await transaction.CommitAsync();
                    _logger.LogInformation($"Orden de compra creada: {orden.NumeroOrden}");
                    return orden;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Error al crear orden de compra: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Obtiene una orden por ID
        /// </summary>
        public async Task<OrdenCompra> ObtenerOrdenPorIdAsync(int ordenId)
        {
            return await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(o => o.OrdenID == ordenId);
        }

        /// <summary>
        /// Obtiene todas las órdenes de compra
        /// </summary>
        public async Task<List<OrdenCompra>> ObtenerTodasLasOrdenesAsync()
        {
            return await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene órdenes por estado
        /// </summary>
        public async Task<List<OrdenCompra>> ObtenerOrdenesPorEstadoAsync(string estado)
        {
            return await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                .Where(o => o.Estado == estado)
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene órdenes por proveedor
        /// </summary>
        public async Task<List<OrdenCompra>> ObtenerOrdenesPorProveedorAsync(int proveedorId)
        {
            return await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                .Where(o => o.ProveedorID == proveedorId)
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();
        }

        /// <summary>
        /// Actualiza una orden de compra
        /// </summary>
        public async Task<OrdenCompra> ActualizarOrdenAsync(OrdenCompra orden)
        {
            var ordenExistente = await _context.OrdenesCompra.FindAsync(orden.OrdenID);
            if (ordenExistente == null)
                throw new InvalidOperationException("Orden no encontrada");

            ordenExistente.FechaEntregaEstimada = orden.FechaEntregaEstimada;
            ordenExistente.Observaciones = orden.Observaciones;
            ordenExistente.FechaModificacion = DateTime.Now;
            ordenExistente.ModificadoPor = orden.ModificadoPor;

            _context.OrdenesCompra.Update(ordenExistente);
            await _context.SaveChangesAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarOperacionAsync(
                usuarioId: 1,
                nombreUsuario: orden.ModificadoPor,
                modulo: "OrdenesCompra",
                tipoOperacion: "UPDATE",
                idRegistro: orden.OrdenID,
                nombreRegistro: orden.NumeroOrden,
                descripcion: $"Orden actualizada por {orden.ModificadoPor}",
                ip: "127.0.0.1"
            );

            return ordenExistente;
        }

        /// <summary>
        /// Cancela una orden de compra
        /// </summary>
        public async Task<bool> CancelarOrdenAsync(int ordenId)
        {
            var orden = await _context.OrdenesCompra.FindAsync(ordenId);
            if (orden == null)
                return false;

            orden.Estado = "Cancelada";
            orden.FechaModificacion = DateTime.Now;

            _context.OrdenesCompra.Update(orden);
            await _context.SaveChangesAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarOperacionAsync(
                usuarioId: 1,
                nombreUsuario: "sistema",
                modulo: "OrdenesCompra",
                tipoOperacion: "UPDATE",
                idRegistro: ordenId,
                nombreRegistro: orden.NumeroOrden,
                descripcion: $"Orden cancelada",
                ip: "127.0.0.1"
            );

            return true;
        }

        /// <summary>
        /// Confirma una orden de compra
        /// </summary>
        public async Task<bool> ConfirmarOrdenAsync(int ordenId)
        {
            var orden = await _context.OrdenesCompra.FindAsync(ordenId);
            if (orden == null)
                return false;

            orden.Estado = "Confirmada";
            orden.FechaModificacion = DateTime.Now;

            _context.OrdenesCompra.Update(orden);
            await _context.SaveChangesAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarOperacionAsync(
                usuarioId: 1,
                nombreUsuario: "sistema",
                modulo: "OrdenesCompra",
                tipoOperacion: "UPDATE",
                idRegistro: ordenId,
                nombreRegistro: orden.NumeroOrden,
                descripcion: $"Orden confirmada",
                ip: "127.0.0.1"
            );

            return true;
        }

        /// <summary>
        /// Obtiene órdenes pendientes de entrega
        /// </summary>
        public async Task<List<OrdenCompra>> ObtenerOrdenesPendientesDeEntregaAsync()
        {
            return await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Where(o => o.Estado == "Confirmada" || o.Estado == "Pendiente")
                .OrderByDescending(o => o.FechaEntregaEstimada)
                .ToListAsync();
        }

        /// <summary>
        /// Genera un número de orden único
        /// </summary>
        private string GenerarNumeroOrden()
        {
            var ultimaOrden = _context.OrdenesCompra
                .OrderByDescending(o => o.OrdenID)
                .FirstOrDefault();

            int numero = (ultimaOrden?.OrdenID ?? 0) + 1;
            return $"ORD-{DateTime.Now:yyyy}-{numero:D6}";
        }
    }
}
