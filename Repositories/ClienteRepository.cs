public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;
    public ClienteRepository(AppDbContext context) => _context = context;

    public List<Cliente> GetAll() => _context.Clientes.OrderByDescending(x => x.Id).ToList();

    public Cliente? GetById(int id) => _context.Clientes.FirstOrDefault(x => x.Id == id);

    public Cliente Add(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        _context.SaveChanges();
        return cliente;
    }
}
