using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
namespace WebApplication1.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PrescriptionMedicament>(entity =>
        {
            entity.HasKey(e => new { e.PrescriptionId, e.MedicamentId });

            entity.HasOne(e => e.Prescription)
                .WithMany(p => p.PrescriptionMedicaments)
                .HasForeignKey(e => e.PrescriptionId);

            entity.HasOne(e => e.Medicament)
                .WithMany(m => m.PrescriptionMedicaments)
                .HasForeignKey(e => e.MedicamentId);
        });
    }

    
}