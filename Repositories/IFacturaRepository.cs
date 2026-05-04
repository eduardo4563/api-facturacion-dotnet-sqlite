public interface IFacturaRepository
{
    List<Factura> GetAll();
    Factura Add(Factura factura);
    int GetNextNumber(string serie);
    Factura Anular(int id);
}
