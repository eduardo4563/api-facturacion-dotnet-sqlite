using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("resumen")]
    public IActionResult Resumen()
    {
        var facturasLista = _context.Facturas.ToList();
        var productosLista = _context.Productos.ToList();

        var facturasActivas = facturasLista.Where(x => !x.Anulada).ToList();

        var totalVentas = facturasActivas.Sum(x => x.Total);
        var facturas = facturasActivas.Count;
        var facturasAnuladas = facturasLista.Count(x => x.Anulada);
        var clientes = _context.Clientes.Count();
        var productos = productosLista.Count;
        var bajoStock = productosLista.Count(x => x.Stock <= 5);
        var sinStock = productosLista.Count(x => x.Stock == 0);

        // Calculate this month's sales
        var hoy = DateTime.UtcNow;
        var ventasMesActual = facturasActivas
            .Where(x => x.Fecha.Year == hoy.Year && x.Fecha.Month == hoy.Month)
            .Sum(x => x.Total);

        var ventasMesAnterior = facturasActivas
            .Where(x => x.Fecha.Year == hoy.AddMonths(-1).Year && x.Fecha.Month == hoy.AddMonths(-1).Month)
            .Sum(x => x.Total);

        return Ok(new
        {
            totalVentas,
            facturas,
            facturasAnuladas,
            clientes,
            productos,
            bajoStock,
            sinStock,
            ventasMesActual,
            ventasMesAnterior
        });
    }

    [HttpGet("ventas-mensuales")]
    public IActionResult VentasMensuales()
    {
        var meses = Enumerable.Range(0, 6)
            .Select(i => DateTime.UtcNow.AddMonths(-i))
            .Reverse()
            .ToList();

        var facturas = _context.Facturas
            .Where(f => !f.Anulada && f.Fecha >= DateTime.UtcNow.AddMonths(-6))
            .ToList();

        var data = meses.Select(m => new
        {
            mes = m.ToString("MMM yyyy"),
            total = facturas
                .Where(f => f.Fecha.Year == m.Year && f.Fecha.Month == m.Month)
                .Sum(f => (double)f.Total),
            cantidad = facturas
                .Count(f => f.Fecha.Year == m.Year && f.Fecha.Month == m.Month)
        }).ToList();

        return Ok(data);
    }

    [HttpGet("top-clientes")]
    public IActionResult TopClientes()
    {
        var top = _context.Facturas
            .Where(f => !f.Anulada)
            .Include(f => f.Cliente)
            .ToList()
            .GroupBy(f => f.ClienteId)
            .Select(g => new
            {
                clienteId = g.Key,
                nombre = g.First().Cliente?.Nombre ?? $"Cliente #{g.Key}",
                totalComprado = g.Sum(f => f.Total),
                cantidadFacturas = g.Count()
            })
            .OrderByDescending(x => x.totalComprado)
            .Take(5)
            .ToList();

        return Ok(top);
    }
}
