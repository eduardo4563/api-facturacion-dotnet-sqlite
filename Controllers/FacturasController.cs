using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FacturasController : ControllerBase
{
    private readonly FacturaService _service;
    public FacturasController(FacturaService service) => _service = service;

    [HttpGet]
    public IActionResult Get() => Ok(_service.GetAll());

    [HttpPost("emitir")]
    public IActionResult Emitir([FromBody] FacturaCreateDto dto) => Ok(_service.Emitir(dto));
}
