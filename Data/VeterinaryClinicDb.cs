using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VeterinaryClinic.Models;

namespace VeterinaryClinic.Data
{
    public class VeterinaryClinicDb : IdentityDbContext <Owner>
    {
        public VeterinaryClinicDb(DbContextOptions<VeterinaryClinicDb> options) : base(options) { }
        public DbSet<Pets> Pets { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pets>().HasMany(p => p.Vaccine).WithMany(v => v.Pets);

        }
    }
}
