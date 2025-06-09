using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LibrarySystem.DbContexts
{
    public class LibraryContext(DbContextOptions<LibraryContext> options) : DbContext(options)
    {
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            // ISBN should always consist of 13 digits
            modelBuilder.Entity<Book>()
                .Property(x => x.ISBN)
                .IsRequired()
                .HasMaxLength(13)
                .IsFixedLength();

            // ISBN should be unique
            modelBuilder.Entity<Book>()
                .HasIndex(x => x.ISBN)
                .IsUnique();
        }
    }
}
