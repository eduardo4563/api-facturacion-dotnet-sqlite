using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly ProductoService _service;
    public ProductosController(ProductoService service) => _service = service;

    [HttpGet]
    public IActionResult Get() => Ok(_service.GetAll());

    [HttpPost]
    public IActionResult Post([FromBody] ProductoCreateDto dto) => Ok(_service.Add(dto));
}
