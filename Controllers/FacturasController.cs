using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FacturasController : ControllerBase
{
    private readonly FacturaService _service;

    public FacturasController(FacturaService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_service.GetAll());
    }

    [HttpPost("emitir")]
    public IActionResult Emitir([FromBody] FacturaCreateDto dto)
    {
        var factura = _service.Emitir(dto);
        return Ok(factura);
    }

    [HttpPatch("{id}/anular")]
    public IActionResult Anular(int id)
    {
        var factura = _service.Anular(id);
        return Ok(factura);
    }

    [HttpGet("{id}/html")]
    [Produces("text/html")]
    public IActionResult VerFacturaHtml(int id)
    {
        var factura = _service.GetById(id);

        var culture = CultureInfo.GetCultureInfo("es-PE");

        string Encode(string? value) =>
            WebUtility.HtmlEncode(value ?? string.Empty);

        string Money(decimal value) =>
            value.ToString("N2", culture);

        var numero = $"{factura.Serie}-{factura.Numero:000000}";
        var fecha = factura.Fecha.ToString("dd/MM/yyyy HH:mm", culture);
        var clienteNombre = Encode(
            factura.Cliente?.Nombre ?? $"Cliente #{factura.ClienteId}"
        );
        var clienteDocumento = Encode(factura.Cliente?.Documento ?? "-");
        var clienteEmail = Encode(factura.Cliente?.Email ?? "-");
        var estadoLabel = factura.Anulada ? "ANULADA" : "EMITIDA";
        var estadoColor = factura.Anulada ? "#fee2e2" : "#dcfce7";
        var estadoTextColor = factura.Anulada ? "#991b1b" : "#166534";

        var filas = new StringBuilder();

        foreach (var d in factura.Detalles)
        {
            var productoNombre = Encode(
                d.Producto?.Nombre ?? $"Producto #{d.ProductoId}"
            );

            var productoCodigo = Encode(d.Producto?.Codigo ?? "SERV");

            filas.Append($@"
<tr>
    <td>
        <b>{productoNombre}</b>
        <br><span>{productoCodigo}</span>
    </td>
    <td>{d.Cantidad}</td>
    <td>S/ {Money(d.PrecioUnitario)}</td>
    <td><b>S/ {Money(d.Total)}</b></td>
</tr>");
        }

        var anulacionBanner = factura.Anulada
            ? $@"
<div class='warning danger'>
    <b>FACTURA ANULADA</b>
    <span>Anulada el {factura.FechaAnulacion?.ToString("dd/MM/yyyy HH:mm", culture) ?? "-"}.</span>
</div>"
            : string.Empty;

        var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Factura {Encode(numero)}</title>
<style>
* {{ box-sizing: border-box; }}
body {{
    margin: 0;
    font-family: Arial, sans-serif;
    background: linear-gradient(135deg, #ecfdf5, #f8fafc, #eef2ff);
    color: #0f172a;
    padding: 40px;
}}
.invoice {{
    max-width: 920px;
    margin: auto;
    background: white;
    border-radius: 28px;
    overflow: hidden;
    box-shadow: 0 30px 90px rgba(15, 23, 42, .16);
}}
.top {{
    background: #020617;
    color: white;
    padding: 34px;
    display: flex;
    justify-content: space-between;
    gap: 20px;
}}
.brand {{ display: flex; gap: 16px; align-items: center; }}
.logo {{
    width: 58px;
    height: 58px;
    border-radius: 18px;
    background: linear-gradient(135deg, #10b981, #06b6d4);
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 900;
    font-size: 22px;
}}
h1 {{ margin: 0; font-size: 36px; }}
.muted {{ color: #94a3b8; margin-top: 6px; }}
.status {{
    display: inline-block;
    background: {estadoColor};
    color: {estadoTextColor};
    padding: 8px 14px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 900;
}}
.content {{ padding: 34px; }}
.grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 18px; }}
.box {{
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 18px;
    padding: 20px;
}}
.box h3 {{ margin-top: 0; color: #059669; }}
.warning {{
    border-radius: 16px;
    padding: 16px 22px;
    margin-bottom: 22px;
    display: flex;
    flex-direction: column;
    gap: 6px;
}}
.warning.demo {{
    background: #fff7ed;
    border: 1px solid #fdba74;
    color: #9a3412;
}}
.warning.danger {{
    background: #fee2e2;
    border: 1px solid #fca5a5;
    color: #991b1b;
}}
table {{ width: 100%; border-collapse: collapse; margin-top: 28px; }}
th {{
    text-align: left;
    background: #f1f5f9;
    padding: 14px;
    font-size: 13px;
    color: #475569;
}}
td {{ padding: 16px 14px; border-bottom: 1px solid #e2e8f0; }}
td span {{ color: #64748b; font-size: 12px; }}
.summary {{ margin-top: 28px; display: flex; justify-content: flex-end; }}
.summary-box {{ width: 330px; background: #f8fafc; border-radius: 20px; padding: 20px; }}
.line {{ display: flex; justify-content: space-between; margin-bottom: 12px; }}
.total {{
    border-top: 2px solid #10b981;
    padding-top: 14px;
    margin-top: 12px;
    font-size: 26px;
    font-weight: 900;
    color: #059669;
}}
.actions {{
    display: flex;
    justify-content: flex-end;
    align-items: center;
    margin-top: 28px;
}}
button {{
    background: #059669;
    color: white;
    border: none;
    padding: 13px 20px;
    border-radius: 14px;
    font-weight: 900;
    cursor: pointer;
}}
.footer {{ margin-top: 28px; color: #64748b; font-size: 13px; }}
@media (max-width: 700px) {{
    body {{ padding: 12px; }}
    .top {{ flex-direction: column; }}
    .grid {{ grid-template-columns: 1fr; }}
    .summary-box {{ width: 100%; }}
}}
@media print {{
    body {{ background: white; padding: 0; }}
    .invoice {{ box-shadow: none; border-radius: 0; }}
    .actions {{ display: none; }}
}}
</style>
</head>
<body>
<div class='invoice'>
    <div class='top'>
        <div class='brand'>
            <div class='logo'>EM</div>
            <div>
                <h1>FACTURA</h1>
                <div class='muted'>Sistema demostrativo de facturacion</div>
            </div>
        </div>
        <div style='text-align:right'>
            <span class='status'>{estadoLabel}</span>
            <h2>{Encode(numero)}</h2>
            <div class='muted'>{fecha}</div>
        </div>
    </div>

    <div class='content'>
        <div class='warning demo'>
            <b>DOCUMENTO DEMOSTRATIVO</b>
            <span>No es un comprobante electronico emitido ante SUNAT.</span>
        </div>

        {anulacionBanner}

        <div class='grid'>
            <div class='box'>
                <h3>Cliente</h3>
                <p><b>{clienteNombre}</b></p>
                <p>Documento: {clienteDocumento}</p>
                <p>Email: {clienteEmail}</p>
            </div>
            <div class='box'>
                <h3>Proyecto</h3>
                <p><b>API REST de Facturacion</b></p>
                <p>ASP.NET Core 8 + SQLite + JWT</p>
                <p>Proyecto de portafolio</p>
            </div>
        </div>

        <table>
            <thead>
                <tr>
                    <th>Descripcion</th>
                    <th>Cantidad</th>
                    <th>Precio</th>
                    <th>Total</th>
                </tr>
            </thead>
            <tbody>{filas}</tbody>
        </table>

        <div class='summary'>
            <div class='summary-box'>
                <div class='line'><span>Subtotal</span><b>S/ {Money(factura.SubTotal)}</b></div>
                <div class='line'><span>IGV 18%</span><b>S/ {Money(factura.Igv)}</b></div>
                <div class='line total'><span>Total</span><span>S/ {Money(factura.Total)}</span></div>
            </div>
        </div>

        <div class='actions'>
            <button onclick='window.print()'>Imprimir / Guardar PDF</button>
        </div>

        <div class='footer'>
            Documento generado por una API de demostracion construida con .NET 8,
            Entity Framework Core, SQLite y autenticacion JWT.
        </div>
    </div>
</div>
</body>
</html>";

        return Content(html, "text/html; charset=utf-8");
    }
}
