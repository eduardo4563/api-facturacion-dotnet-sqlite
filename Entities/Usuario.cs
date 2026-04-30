public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = ""; // Demo. En producción usar hash.
    public string Rol { get; set; } = "Admin";
}
