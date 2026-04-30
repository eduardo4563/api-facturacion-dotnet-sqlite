public class FacturaCreateDto
{
    public int ClienteId { get; set; }
    public List<FacturaDetalleDto> Detalles { get; set; } = new();
}
