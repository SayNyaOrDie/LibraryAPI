using Microsoft.EntityFrameworkCore;
using WebApplication3.Enums;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookDamage> BookDamages { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Visitor>()
                .HasMany(v => v.Transactions)
                .WithOne()
                .HasForeignKey(t => t.VisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Visitor>()
                .Property(v => v.Debt)
                .HasColumnType("decimal(18,2)");        

            modelBuilder.Entity<Transaction>()
                .HasOne<Book>()
                .WithMany()
                .HasForeignKey(t => t.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Book>()
                .Property(b => b.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (BookStatus)Enum.Parse(typeof(BookStatus), v)
                );

            modelBuilder.Entity<BookDamage>()
                .Property(d => d.Rate)
                .HasConversion(
                    v => v.ToString(),
                    v => (BookDamageRate)Enum.Parse(typeof(BookDamageRate), v)
                );

            modelBuilder.Entity<Transaction>()
                .Property(s => s.TransactionStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (TransactionStatus)Enum.Parse(typeof(TransactionStatus), v)
                );

            modelBuilder.Entity<Book>()
                .OwnsOne(b => b.Author, a =>
                {
                    a.Property(p => p.Name).HasColumnName("Author_Name").IsRequired();
                    a.Property(p => p.Surname).HasColumnName("Author_Surname").IsRequired();
                });

        }
    }
}
