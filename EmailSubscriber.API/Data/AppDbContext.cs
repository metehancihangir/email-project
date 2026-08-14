using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailSubscriber.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignRecipient> CampaignRecipients { get; set; }
    public DbSet<TrackedLink> TrackedLinks { get; set; }
    public DbSet<PastAITopic> PastAITopics { get; set; }
    public DbSet<CampaignFeedback> CampaignFeedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrackedLink>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.OriginalUrl).IsRequired().HasMaxLength(2048);
            entity.Property(t => t.LinkToken).IsRequired().HasMaxLength(64);
            entity.HasIndex(t => t.LinkToken).IsUnique();
        });
        modelBuilder.Entity<Subscriber>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(s => s.Email).IsUnique();
            entity.Property(s => s.Name).HasMaxLength(100);
            entity.Property(s => s.ConfirmationToken).HasMaxLength(255);
            entity.Property(s => s.UnsubscribeToken).HasMaxLength(64);
            entity.HasIndex(s => s.UnsubscribeToken).IsUnique();

            // Faz 2 — 2.2.2: Cooldown kontrol sorgusu için composite index.
            // idx_subscribers_email_lastrequested
            entity.HasIndex(s => new { s.Email, s.LastCodeRequestedAt })
                  .HasDatabaseName("idx_subscribers_email_lastrequested");
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Subject).IsRequired().HasMaxLength(255);
            entity.Property(c => c.HtmlBody).IsRequired(); // LONGTEXT will be mapped implicitly or explicitly based on provider
            entity.Property(c => c.Category).HasMaxLength(50);
            entity.Property(c => c.CoverImageUrl).HasMaxLength(2048);
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

        modelBuilder.Entity<PastAITopic>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Category).IsRequired().HasMaxLength(50);
            entity.Property(p => p.TopicName).IsRequired().HasMaxLength(255);
            entity.HasIndex(p => p.Category);
        });

        modelBuilder.Entity<CampaignFeedback>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.HasOne(f => f.Campaign)
                  .WithMany(c => c.Feedbacks)
                  .HasForeignKey(f => f.CampaignId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Subscriber)
                  .WithMany()
                  .HasForeignKey(f => f.SubscriberId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => new { f.CampaignId, f.SubscriberId }).IsUnique();
        });
    }
}
