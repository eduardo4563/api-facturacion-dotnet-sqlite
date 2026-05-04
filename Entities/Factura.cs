public class Factura
{
    public int Id { get; set; }
    public string Serie { get; set; } = "F001";
    public int Numero { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public bool Anulada { get; set; } = false;
    public DateTime? FechaAnulacion { get; set; }
    public List<FacturaDetalle> Detalles { get; set; } = new();
}
