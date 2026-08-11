using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailSubscriber.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscriber>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(s => s.Email).IsUnique();
            entity.Property(s => s.Name).HasMaxLength(100);
            entity.Property(s => s.ConfirmationToken).HasMaxLength(255);
        });
    }
}
