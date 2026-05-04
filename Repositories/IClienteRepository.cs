public interface IClienteRepository
{
    List<Cliente> GetAll();
    Cliente? GetById(int id);
    Cliente Add(Cliente cliente);
    bool Delete(int id);
}
