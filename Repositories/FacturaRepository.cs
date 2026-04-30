using Microsoft.EntityFrameworkCore;

public class FacturaRepository : IFacturaRepository
{
    private readonly AppDbContext _context;
    public FacturaRepository(AppDbContext context) => _context = context;

    public List<Factura> GetAll()
    {
        return _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Detalles)
            .ThenInclude(d => d.Producto)
            .OrderByDescending(f => f.Id)
            .ToList();
    }

    public Factura Add(Factura factura)
    {
        _context.Facturas.Add(factura);
        _context.SaveChanges();
        return factura;
    }

    public int GetNextNumber(string serie)
    {
        var last = _context.Facturas
            .Where(x => x.Serie == serie)
            .OrderByDescending(x => x.Numero)
            .FirstOrDefault();

        return last == null ? 1 : last.Numero + 1;
    }
}
