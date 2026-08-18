using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Factura> Facturas => Set<Factura>();

    public DbSet<FacturaDetalle> FacturaDetalles => Set<FacturaDetalle>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // =====================================================
        // RELACIONES
        // =====================================================

        modelBuilder.Entity<Factura>()
            .HasMany(f => f.Detalles)
            .WithOne()
            .HasForeignKey(d => d.FacturaId);


        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Cliente)
            .WithMany()
            .HasForeignKey(f => f.ClienteId);


        modelBuilder.Entity<FacturaDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId);


        // =====================================================
        // CLIENTE DEMO
        // =====================================================

        modelBuilder.Entity<Cliente>().HasData(
            new Cliente
            {
                Id = 1,
                Nombre = "Cliente Demo SAC",
                Documento = "20600000001",
                Email = "cliente@demo.com",
                Telefono = "999999999"
            }
        );


        // =====================================================
        // PRODUCTOS DEMO
        // =====================================================

        modelBuilder.Entity<Producto>().HasData(

            new Producto
            {
                Id = 1,
                Codigo = "SERV-001",
                Nombre = "Servicio de desarrollo backend",
                Precio = 850,
                Stock = 100
            },

            new Producto
            {
                Id = 2,
                Codigo = "SIST-001",
                Nombre = "Sistema web empresarial",
                Precio = 1500,
                Stock = 50
            }

        );

        // El usuario admin NO se crea aquí.
        // Se crea desde Program.cs con la contraseña hasheada.
    }
}
