using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailSubscriber.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignRecipient> CampaignRecipients => Set<CampaignRecipient>();

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

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Subject).IsRequired().HasMaxLength(255);
            entity.Property(c => c.HtmlBody).IsRequired(); // LONGTEXT will be mapped implicitly or explicitly based on provider
        });

        modelBuilder.Entity<CampaignRecipient>(entity =>
        {
            entity.HasKey(cr => cr.Id);
            entity.Property(cr => cr.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(cr => cr.Campaign)
                  .WithMany(c => c.Recipients)
                  .HasForeignKey(cr => cr.CampaignId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cr => cr.Subscriber)
                  .WithMany()
                  .HasForeignKey(cr => cr.SubscriberId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
