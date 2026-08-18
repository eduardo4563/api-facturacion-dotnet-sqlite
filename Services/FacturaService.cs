using Microsoft.EntityFrameworkCore;

public class FacturaService
{
    private readonly IFacturaRepository _facturaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly AppDbContext _context;

    public FacturaService(
        IFacturaRepository facturaRepository,
        IProductoRepository productoRepository,
        IClienteRepository clienteRepository,
        AppDbContext context)
    {
        _facturaRepository = facturaRepository;
        _productoRepository = productoRepository;
        _clienteRepository = clienteRepository;
        _context = context;
    }

    public List<Factura> GetAll() => _facturaRepository.GetAll();

    public Factura GetById(int id)
    {
        return _facturaRepository.GetAll()
            .FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException("La factura no existe.");
    }

    public Factura Emitir(FacturaCreateDto dto)
    {
        if (_clienteRepository.GetById(dto.ClienteId) == null)
        {
            throw new KeyNotFoundException("El cliente no existe.");
        }

        if (dto.Detalles == null || dto.Detalles.Count == 0)
        {
            throw new InvalidOperationException(
                "La factura debe tener al menos un producto."
            );
        }

        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var factura = new Factura
            {
                Serie = "F001",
                Numero = _facturaRepository.GetNextNumber("F001"),
                ClienteId = dto.ClienteId,
                Fecha = DateTime.UtcNow
            };

            decimal totalConIgv = 0;

            foreach (var item in dto.Detalles)
            {
                if (item.Cantidad <= 0)
                {
                    throw new InvalidOperationException(
                        "La cantidad debe ser mayor a cero."
                    );
                }

                var producto = _productoRepository.GetById(item.ProductoId)
                    ?? throw new KeyNotFoundException(
                        $"El producto con ID {item.ProductoId} no existe."
                    );

                if (producto.Stock < item.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{producto.Nombre}'. " +
                        $"Disponible: {producto.Stock}."
                    );
                }

                var totalDetalle = Math.Round(
                    producto.Precio * item.Cantidad,
                    2
                );

                factura.Detalles.Add(new FacturaDetalle
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Total = totalDetalle
                });

                producto.Stock -= item.Cantidad;
                _productoRepository.Update(producto);

                totalConIgv += totalDetalle;
            }

            factura.SubTotal = Math.Round(totalConIgv / 1.18m, 2);
            factura.Igv = Math.Round(totalConIgv - factura.SubTotal, 2);
            factura.Total = Math.Round(totalConIgv, 2);

            var creada = _facturaRepository.Add(factura);

            transaction.Commit();
            return creada;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Factura Anular(int id)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var factura = GetById(id);

            if (factura.Anulada)
            {
                throw new InvalidOperationException(
                    "La factura ya esta anulada."
                );
            }

            foreach (var detalle in factura.Detalles)
            {
                var producto = _productoRepository.GetById(detalle.ProductoId);

                if (producto == null)
                {
                    throw new InvalidOperationException(
                        $"No se puede restaurar el stock porque el producto " +
                        $"con ID {detalle.ProductoId} no existe."
                    );
                }

                producto.Stock += detalle.Cantidad;
                _productoRepository.Update(producto);
            }

            var anulada = _facturaRepository.Anular(id);

            transaction.Commit();
            return anulada;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
