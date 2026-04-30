public class ProductoService
{
    private readonly IProductoRepository _repository;
    public ProductoService(IProductoRepository repository) => _repository = repository;

    public List<Producto> GetAll() => _repository.GetAll();

    public Producto Add(ProductoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new Exception("El nombre del producto es obligatorio.");
        if (dto.Precio <= 0) throw new Exception("El precio debe ser mayor a cero.");
        if (dto.Stock < 0) throw new Exception("El stock no puede ser negativo.");

        var producto = new Producto
        {
            Codigo = dto.Codigo.Trim(),
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        return _repository.Add(producto);
    }
}
