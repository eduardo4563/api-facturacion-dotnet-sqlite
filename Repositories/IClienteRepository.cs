public interface IClienteRepository
{
    List<Cliente> GetAll();
    Cliente? GetById(int id);
    Cliente Add(Cliente cliente);
}
