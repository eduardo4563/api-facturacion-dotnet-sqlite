public class FacturaService
{
    private readonly IFacturaRepository _facturaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IClienteRepository _clienteRepository;

    public FacturaService(
        IFacturaRepository facturaRepository,
        IProductoRepository productoRepository,
        IClienteRepository clienteRepository)
    {
        _facturaRepository = facturaRepository;
        _productoRepository = productoRepository;
        _clienteRepository = clienteRepository;
    }

    public List<Factura> GetAll() => _facturaRepository.GetAll();

    public Factura Emitir(FacturaCreateDto dto)
    {
        if (_clienteRepository.GetById(dto.ClienteId) == null)
            throw new Exception("El cliente no existe.");

        if (dto.Detalles == null || dto.Detalles.Count == 0)
            throw new Exception("La factura debe tener al menos un producto.");

        var factura = new Factura
        {
            Serie = "F001",
            Numero = _facturaRepository.GetNextNumber("F001"),
            ClienteId = dto.ClienteId,
            Fecha = DateTime.UtcNow
        };

        decimal subtotal = 0;

        foreach (var item in dto.Detalles)
        {
            if (item.Cantidad <= 0)
                throw new Exception("La cantidad debe ser mayor a cero.");

            var producto = _productoRepository.GetById(item.ProductoId)
                ?? throw new Exception($"El producto con ID {item.ProductoId} no existe.");

            if (producto.Stock < item.Cantidad)
                throw new Exception($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}.");

            var totalDetalle = producto.Precio * item.Cantidad;

            factura.Detalles.Add(new FacturaDetalle
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                Total = totalDetalle
            });

            producto.Stock -= item.Cantidad;
            _productoRepository.Update(producto);

            subtotal += totalDetalle;
        }

        factura.SubTotal = Math.Round(subtotal / 1.18m, 2);
        factura.Igv = Math.Round(subtotal - factura.SubTotal, 2);
        factura.Total = Math.Round(subtotal, 2);

        return _facturaRepository.Add(factura);
    }

    public Factura Anular(int id)
    {
        var factura = _facturaRepository.GetAll().FirstOrDefault(x => x.Id == id)
            ?? throw new Exception("La factura no existe.");

        if (factura.Anulada)
            throw new Exception("La factura ya está anulada.");

        // Restore stock for each detail
        foreach (var detalle in factura.Detalles)
        {
            var producto = _productoRepository.GetById(detalle.ProductoId);
            if (producto != null)
            {
                producto.Stock += detalle.Cantidad;
                _productoRepository.Update(producto);
            }
        }

        return _facturaRepository.Anular(id);
    }
}
