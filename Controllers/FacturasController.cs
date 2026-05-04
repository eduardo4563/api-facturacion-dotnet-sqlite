using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [AllowAnonymous]
    [HttpGet("{id}/html")]
    public IActionResult VerFacturaHtml(int id)
    {
        var factura = _service.GetAll().FirstOrDefault(x => x.Id == id);

        if (factura == null)
            return NotFound();

        var numero = $"{factura.Serie}-{factura.Numero.ToString("000000")}";
        var fecha = factura.Fecha.ToString("dd/MM/yyyy HH:mm");
        var clienteNombre = factura.Cliente?.Nombre ?? $"Cliente #{factura.ClienteId}";
        var clienteDocumento = factura.Cliente?.Documento ?? "-";
        var clienteEmail = factura.Cliente?.Email ?? "-";
        var estadoLabel = factura.Anulada ? "ANULADA" : "EMITIDA";
        var estadoColor = factura.Anulada ? "#fee2e2" : "#dcfce7";
        var estadoTextColor = factura.Anulada ? "#991b1b" : "#166534";

        var filas = new StringBuilder();

        foreach (var d in factura.Detalles)
        {
            filas.Append($@"
<tr>
    <td>
        <b>{d.Producto?.Nombre ?? $"Producto #{d.ProductoId}"}</b>
        <br><span>{d.Producto?.Codigo ?? "SERV"}</span>
    </td>
    <td>{d.Cantidad}</td>
    <td>S/ {d.PrecioUnitario:N2}</td>
    <td><b>S/ {d.Total:N2}</b></td>
</tr>");
        }

        var anulacionBanner = factura.Anulada ? $@"
<div style='background:#fee2e2;border:2px solid #fca5a5;border-radius:16px;padding:16px 22px;margin-bottom:28px;display:flex;align-items:center;gap:12px'>
    <span style='font-size:24px'>⚠️</span>
    <div>
        <b style='color:#991b1b;font-size:15px'>FACTURA ANULADA</b>
        <p style='margin:4px 0 0;color:#b91c1c;font-size:13px'>Anulada el {factura.FechaAnulacion?.ToString("dd/MM/yyyy HH:mm") ?? "-"}. Este documento no tiene validez fiscal.</p>
    </div>
</div>" : "";

        var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
<meta charset='UTF-8'>
<title>Factura {numero}</title>
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
    width: 58px; height: 58px; border-radius: 18px;
    background: linear-gradient(135deg, #10b981, #06b6d4);
    display: flex; align-items: center; justify-content: center;
    font-weight: 900; font-size: 22px;
}}
h1 {{ margin: 0; font-size: 36px; }}
.muted {{ color: #94a3b8; margin-top: 6px; }}
.status {{
    display: inline-block;
    background: {estadoColor};
    color: {estadoTextColor};
    padding: 8px 14px; border-radius: 999px;
    font-size: 12px; font-weight: 900;
}}
.content {{ padding: 34px; }}
.grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 18px; }}
.box {{ background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 18px; padding: 20px; }}
.box h3 {{ margin-top: 0; color: #059669; }}
table {{ width: 100%; border-collapse: collapse; margin-top: 28px; }}
th {{ text-align: left; background: #f1f5f9; padding: 14px; font-size: 13px; color: #475569; }}
td {{ padding: 16px 14px; border-bottom: 1px solid #e2e8f0; }}
td span {{ color: #64748b; font-size: 12px; }}
.summary {{ margin-top: 28px; display: flex; justify-content: flex-end; }}
.summary-box {{ width: 330px; background: #f8fafc; border-radius: 20px; padding: 20px; }}
.line {{ display: flex; justify-content: space-between; margin-bottom: 12px; }}
.total {{ border-top: 2px solid #10b981; padding-top: 14px; margin-top: 12px; font-size: 26px; font-weight: 900; color: #059669; }}
.qr {{
    width: 92px; height: 92px;
    background:
        linear-gradient(90deg, #111827 10px, transparent 10px),
        linear-gradient(#111827 10px, transparent 10px);
    background-size: 23px 23px;
    border-radius: 12px; opacity: .9;
}}
.actions {{ display: flex; justify-content: space-between; align-items: center; gap: 16px; margin-top: 28px; }}
button {{ background: #059669; color: white; border: none; padding: 13px 20px; border-radius: 14px; font-weight: 900; cursor: pointer; }}
.footer {{ margin-top: 28px; color: #64748b; font-size: 13px; }}
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
                <div class='muted'>Sistema Empresarial de Facturación</div>
            </div>
        </div>
        <div style='text-align:right'>
            <span class='status'>{estadoLabel}</span>
            <h2>{numero}</h2>
            <div class='muted'>{fecha}</div>
        </div>
    </div>

    <div class='content'>
        {anulacionBanner}

        <div class='grid'>
            <div class='box'>
                <h3>Cliente</h3>
                <p><b>{clienteNombre}</b></p>
                <p>Documento: {clienteDocumento}</p>
                <p>Email: {clienteEmail}</p>
            </div>
            <div class='box'>
                <h3>Empresa emisora</h3>
                <p><b>Eduardo Montaño Systems</b></p>
                <p>Backend / Full Stack Developer</p>
                <p>Lima, Perú</p>
            </div>
        </div>

        <table>
            <thead>
                <tr>
                    <th>Descripción</th>
                    <th>Cantidad</th>
                    <th>Precio</th>
                    <th>Total</th>
                </tr>
            </thead>
            <tbody>{filas}</tbody>
        </table>

        <div class='summary'>
            <div class='summary-box'>
                <div class='line'><span>Subtotal</span><b>S/ {factura.SubTotal:N2}</b></div>
                <div class='line'><span>IGV 18%</span><b>S/ {factura.Igv:N2}</b></div>
                <div class='line total'><span>Total</span><span>S/ {factura.Total:N2}</span></div>
            </div>
        </div>

        <div class='actions'>
            <div>
                <div class='qr'></div>
                <div class='footer'>Código de verificación visual</div>
            </div>
            <button onclick='window.print()'>Imprimir / Guardar PDF</button>
        </div>

        <div class='footer'>
            Esta factura fue generada automáticamente por una API .NET con JWT, SQLite y arquitectura en capas.
        </div>
    </div>
</div>
</body>
</html>";

        return Content(html, "text/html");
    }
}
