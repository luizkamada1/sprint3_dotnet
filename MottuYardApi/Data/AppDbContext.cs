using Microsoft.EntityFrameworkCore;
using MottuYardApi.Models;

namespace MottuYardApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Patio> Patios => Set<Patio>();
        public DbSet<Zona> Zonas => Set<Zona>();
        public DbSet<Moto> Motos => Set<Moto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patio>().HasMany(p => p.Zonas).WithOne(z => z.Patio!).HasForeignKey(z => z.PatioId);
            modelBuilder.Entity<Zona>().HasMany(z => z.Motos).WithOne(m => m.Zona!).HasForeignKey(m => m.ZonaId);
        }
    }

    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            if (!db.Patios.Any())
            {
                var sp = new Patio { Nome = "CD São Paulo", Cidade = "São Paulo", Estado = "SP" };
                var rj = new Patio { Nome = "CD Rio de Janeiro", Cidade = "Rio de Janeiro", Estado = "RJ" };
                db.Patios.AddRange(sp, rj);
                db.SaveChanges();

                var z1 = new Zona { Nome = "A1", PatioId = sp.Id };
                var z2 = new Zona { Nome = "A2", PatioId = sp.Id };
                var z3 = new Zona { Nome = "B1", PatioId = rj.Id };
                db.Zonas.AddRange(z1, z2, z3);
                db.SaveChanges();

                db.Motos.AddRange(
                    new Moto { Placa = "ABC1D23", Modelo = "CG 160", Status = "Ativa", ZonaId = z1.Id },
                    new Moto { Placa = "XYZ4E56", Modelo = "NMax 160", Status = "Manutenção", ZonaId = z2.Id },
                    new Moto { Placa = "JKL7M89", Modelo = "Fazer 250", Status = "Ativa", ZonaId = z3.Id }
                );
                db.SaveChanges();
            }
        }
    }
}