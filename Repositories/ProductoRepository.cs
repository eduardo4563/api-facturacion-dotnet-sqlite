public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _context;
    public ProductoRepository(AppDbContext context) => _context = context;

    public List<Producto> GetAll() => _context.Productos.OrderByDescending(x => x.Id).ToList();

    public Producto? GetById(int id) => _context.Productos.FirstOrDefault(x => x.Id == id);

    public Producto Add(Producto producto)
    {
        _context.Productos.Add(producto);
        _context.SaveChanges();
        return producto;
    }

    public void Update(Producto producto)
    {
        _context.Productos.Update(producto);
        _context.SaveChanges();
    }

    public bool Delete(int id)
    {
        var producto = _context.Productos.FirstOrDefault(x => x.Id == id);
        if (producto == null) return false;

        _context.Productos.Remove(producto);
        _context.SaveChanges();
        return true;
    }
}
