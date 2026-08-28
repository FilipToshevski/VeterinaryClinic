using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VeterinaryClinic.Models;

namespace VeterinaryClinic.Data
{
    public class VeterinaryClinicDb : IdentityDbContext<Owner, IdentityRole, string>
    {
        public VeterinaryClinicDb(DbContextOptions<VeterinaryClinicDb> options)
            : base(options) { }

        // Note: Owner records are accessed via the inherited Users property
        // (IdentityDbContext<Owner, ...>), not a separate DbSet, to avoid
        // two different-looking ways to query the same table.
        public DbSet<Pets> Pets { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<PetVaccine> PetVaccines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<PetVaccine>()
                .HasKey(pv => new { pv.PetId, pv.VaccineId });

            modelBuilder.Entity<PetVaccine>()
                .HasOne(pv => pv.Pet)
                .WithMany(p => p.PetVaccines)
                .HasForeignKey(pv => pv.PetId);

            modelBuilder.Entity<PetVaccine>()
                .HasOne(pv => pv.Vaccine)
                .WithMany(v => v.PetVaccines)
                .HasForeignKey(pv => pv.VaccineId);

            
        }
    }
}