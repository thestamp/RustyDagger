using Microsoft.EntityFrameworkCore;
using RustyDagger.Data.Entities;

namespace RustyDagger.Data;

public class DragonCourtDbContext : DbContext
{
    public DragonCourtDbContext(DbContextOptions<DragonCourtDbContext> options) : base(options) { }

    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
    public DbSet<HeroEntity> Heroes => Set<HeroEntity>();
    public DbSet<MailEntity> Mails => Set<MailEntity>();
    public DbSet<RankingEntity> Rankings => Set<RankingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(e =>
        {
            e.HasIndex(a => a.Name).IsUnique();
        });

        modelBuilder.Entity<HeroEntity>(e =>
        {
            e.HasIndex(h => h.Name).IsUnique();
            e.HasOne(h => h.Account)
                .WithMany(a => a.Heroes)
                .HasForeignKey(h => h.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MailEntity>(e =>
        {
            e.HasIndex(m => m.RecipientAccountId);
        });

        modelBuilder.Entity<RankingEntity>(e =>
        {
            e.HasIndex(r => r.Fame).IsDescending();
            e.HasIndex(r => r.Level).IsDescending();
        });
    }
}
