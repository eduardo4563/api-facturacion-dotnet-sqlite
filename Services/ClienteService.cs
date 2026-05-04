public class ClienteService
{
    private readonly IClienteRepository _repository;
    private readonly IFacturaRepository _facturaRepository;

    public ClienteService(IClienteRepository repository, IFacturaRepository facturaRepository)
    {
        _repository = repository;
        _facturaRepository = facturaRepository;
    }

    public List<Cliente> GetAll() => _repository.GetAll();

    public Cliente Add(ClienteCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new Exception("El nombre del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Documento))
            throw new Exception("El documento del cliente es obligatorio.");

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Documento = dto.Documento.Trim(),
            Email = dto.Email?.Trim() ?? string.Empty,
            Telefono = dto.Telefono?.Trim() ?? string.Empty
        };

        return _repository.Add(cliente);
    }

    public void Delete(int id)
    {
        var cliente = _repository.GetById(id)
            ?? throw new Exception($"El cliente con ID {id} no existe.");

        var tieneFacturas = _facturaRepository.GetAll()
            .Any(f => f.ClienteId == id && !f.Anulada);

        if (tieneFacturas)
            throw new Exception("No se puede eliminar un cliente con facturas activas.");

        _repository.Delete(id);
    }
}
