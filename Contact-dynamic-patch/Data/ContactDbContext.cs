using ContactApi.Models;
using Microsoft.EntityFrameworkCore;


namespace ContactApi.Data
{
    public class ContactDbContext : DbContext
    {
        public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options)
        {
        }


        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Contact configuration
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Telephone).IsUnique();

                entity.Property(e => e.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.LastName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Telephone)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                // ✅ One-to-many: Contact -> Addresses
                entity.HasMany(c => c.Addresses)
                      .WithOne(a => a.Contact)
                      .HasForeignKey(a => a.ContactId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Address configuration (optional but good for consistency)
            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(a => a.Street)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.City)
                      .IsRequired()
                      .HasMaxLength(50);

                // Ensure ContactId is required (since Address must belong to a Contact)
                entity.Property(a => a.ContactId)
                      .IsRequired();
            });
        }
    }
}
