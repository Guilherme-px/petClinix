using Microsoft.EntityFrameworkCore;
using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.ValueObjects;

namespace PetClinix.Modules.Pets.Infrastructure.Persistence;

public class PetsDbContext : DbContext
{
    public PetsDbContext(DbContextOptions<PetsDbContext> options) : base(options) { }
    public DbSet<Tutor> Tutors => Set<Tutor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tutor>(entity =>
        {
            entity.ToTable("Tutors");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Cpf).HasConversion(c => c.Value, v => Cpf.Create(v));
            entity.Property(t => t.Name).HasMaxLength(150).IsRequired();
            entity.Property(t => t.Email).HasMaxLength(256);
            entity.Property(t => t.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(t => t.SecondaryPhoneNumber).HasMaxLength(20);
            entity.Property(t => t.ZipCode).HasMaxLength(20).IsRequired();
            entity.Property(t => t.Street).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Number).HasMaxLength(20).IsRequired();
            entity.Property(t => t.Neighborhood).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Complement).HasMaxLength(200);
            entity.Property(t => t.City).HasMaxLength(100).IsRequired();
            entity.Property(t => t.State).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Notes).HasMaxLength(1000);
            entity.HasIndex(t => new { t.ClinicId, t.Cpf }).IsUnique();
        });
    }
} 