using Microsoft.EntityFrameworkCore;
using AccountStatements.Domain.Entities;

namespace AccountStatements.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<AccountStatement> AccountStatements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(250);
                entity.HasQueryFilter(c => !c.IsDeleted);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Description).IsRequired().HasMaxLength(250);
                entity.Property(t => t.Amount).HasColumnType("TEXT");
                entity.HasOne(t => t.Customer)
                    .WithMany()
                    .HasForeignKey(t => t.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasQueryFilter(t => !t.IsDeleted);
            });

            modelBuilder.Entity<AccountStatement>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.StatementMonth).IsRequired().HasMaxLength(7);
                entity.Property(s => s.EmailSentStatus).IsRequired().HasMaxLength(20);
                entity.Property(s => s.StartingBalance).HasColumnType("TEXT");
                entity.Property(s => s.EndingBalance).HasColumnType("TEXT");
                
                entity.HasOne(s => s.Customer)
                    .WithMany()
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasQueryFilter(s => !s.IsDeleted);
            });
        }
    }
}
