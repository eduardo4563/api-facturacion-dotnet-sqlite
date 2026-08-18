public class ProductoService
{
    private readonly IProductoRepository _repository;
    private readonly IFacturaRepository _facturaRepository;

    public ProductoService(
        IProductoRepository repository,
        IFacturaRepository facturaRepository)
    {
        _repository = repository;
        _facturaRepository = facturaRepository;
    }

    public List<Producto> GetAll() => _repository.GetAll();

    public Producto Add(ProductoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo))
            throw new InvalidOperationException(
                "El codigo del producto es obligatorio."
            );

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new InvalidOperationException(
                "El nombre del producto es obligatorio."
            );

        if (dto.Precio <= 0)
            throw new InvalidOperationException(
                "El precio debe ser mayor a cero."
            );

        if (dto.Stock < 0)
            throw new InvalidOperationException(
                "El stock no puede ser negativo."
            );

        var codigo = dto.Codigo.Trim();

        if (_repository.GetAll().Any(x => x.Codigo == codigo))
        {
            throw new InvalidOperationException(
                "Ya existe un producto con ese codigo."
            );
        }

        var producto = new Producto
        {
            Codigo = codigo,
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        return _repository.Add(producto);
    }

    public Producto Update(int id, ProductoUpdateDto dto)
    {
        var producto = _repository.GetById(id)
            ?? throw new KeyNotFoundException(
                $"El producto con ID {id} no existe."
            );

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new InvalidOperationException(
                "El nombre del producto es obligatorio."
            );

        if (dto.Precio <= 0)
            throw new InvalidOperationException(
                "El precio debe ser mayor a cero."
            );

        if (dto.Stock < 0)
            throw new InvalidOperationException(
                "El stock no puede ser negativo."
            );

        producto.Nombre = dto.Nombre.Trim();
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;

        _repository.Update(producto);
        return producto;
    }

    public void Delete(int id)
    {
        _ = _repository.GetById(id)
            ?? throw new KeyNotFoundException(
                $"El producto con ID {id} no existe."
            );

        var usadoEnFacturas = _facturaRepository.GetAll()
            .Any(f => f.Detalles.Any(d => d.ProductoId == id));

        if (usadoEnFacturas)
        {
            throw new InvalidOperationException(
                "No se puede eliminar un producto que ya forma parte de una factura."
            );
        }

        _repository.Delete(id);
    }
}
