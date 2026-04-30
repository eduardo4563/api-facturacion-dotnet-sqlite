public class ClienteService
{
    private readonly IClienteRepository _repository;
    public ClienteService(IClienteRepository repository) => _repository = repository;

    public List<Cliente> GetAll() => _repository.GetAll();

    public Cliente Add(ClienteCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new Exception("El nombre del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Documento)) throw new Exception("El documento del cliente es obligatorio.");

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Documento = dto.Documento.Trim(),
            Email = dto.Email.Trim(),
            Telefono = dto.Telefono.Trim()
        };

        return _repository.Add(cliente);
    }
}
