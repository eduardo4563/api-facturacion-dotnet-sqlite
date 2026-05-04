public interface IProductoRepository
{
    List<Producto> GetAll();
    Producto? GetById(int id);
    Producto Add(Producto producto);
    void Update(Producto producto);
    bool Delete(int id);
}
