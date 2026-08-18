public class ClienteService
{
    private readonly IClienteRepository _repository;
    private readonly IFacturaRepository _facturaRepository;

    public ClienteService(
        IClienteRepository repository,
        IFacturaRepository facturaRepository)
    {
        _repository = repository;
        _facturaRepository = facturaRepository;
    }

    public List<Cliente> GetAll() => _repository.GetAll();

    public Cliente Add(ClienteCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new InvalidOperationException(
                "El nombre del cliente es obligatorio."
            );

        if (string.IsNullOrWhiteSpace(dto.Documento))
            throw new InvalidOperationException(
                "El documento del cliente es obligatorio."
            );

        var documento = dto.Documento.Trim();

        if (_repository.GetAll().Any(x => x.Documento == documento))
        {
            throw new InvalidOperationException(
                "Ya existe un cliente con ese documento."
            );
        }

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Documento = documento,
            Email = dto.Email?.Trim() ?? string.Empty,
            Telefono = dto.Telefono?.Trim() ?? string.Empty
        };

        return _repository.Add(cliente);
    }

    public void Delete(int id)
    {
        _ = _repository.GetById(id)
            ?? throw new KeyNotFoundException(
                $"El cliente con ID {id} no existe."
            );

        var tieneFacturas = _facturaRepository.GetAll()
            .Any(f => f.ClienteId == id);

        if (tieneFacturas)
        {
            throw new InvalidOperationException(
                "No se puede eliminar un cliente que ya tiene facturas registradas."
            );
        }

        _repository.Delete(id);
    }
}
