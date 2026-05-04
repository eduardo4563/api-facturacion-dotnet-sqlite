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

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] ProductoUpdateDto dto)
    {
        var updated = _service.Update(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}
