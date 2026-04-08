using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NutriGuide.Domain.Models;

namespace NutriGuide.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<DailyTarget> DailyTargets => Set<DailyTarget>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<FavoriteMeal> FavoriteMeals => Set<FavoriteMeal>();
    public DbSet<MealLogFavorite> MealLogFavorites => Set<MealLogFavorite>();
    public DbSet<DailyNutritionSummary> DailyNutritionSummaries => Set<DailyNutritionSummary>();
    public DbSet<WellnessLog> WellnessLogs => Set<WellnessLog>();
    public DbSet<AiRecommendation> AiRecommendations => Set<AiRecommendation>();
    public DbSet<WeightProgress> WeightProgress => Set<WeightProgress>();
    public DbSet<WeeklyReport> WeeklyReports => Set<WeeklyReport>();
    public DbSet<MessagingConnection> MessagingConnections => Set<MessagingConnection>();
    public DbSet<MessagingLog> MessagingLogs => Set<MessagingLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        
        builder.Entity<MealLogFavorite>()
            .HasKey(mlf => new { mlf.MealLogId, mlf.FavoriteMealId });

       
        builder.Entity<MealLogFavorite>()
            .HasOne(mlf => mlf.MealLog)
            .WithMany(ml => ml.MealLogFavorites)
            .HasForeignKey(mlf => mlf.MealLogId);

        builder.Entity<MealLogFavorite>()
            .HasOne(mlf => mlf.FavoriteMeal)
            .WithMany(fm => fm.MealLogFavorites)
            .HasForeignKey(mlf => mlf.FavoriteMealId);

        
        builder.Entity<DailyTarget>()
            .HasIndex(dt => new { dt.UserId, dt.TargetDate })
            .IsUnique();

        builder.Entity<DailyNutritionSummary>()
            .HasIndex(dns => new { dns.UserId, dns.SummaryDate })
            .IsUnique();

        builder.Entity<WeightProgress>()
            .HasIndex(wp => new { wp.UserId, wp.RecordedDate })
            .IsUnique();

        builder.Entity<WeeklyReport>()
            .HasIndex(wr => new { wr.UserId, wr.WeekStart })
            .IsUnique();

        builder.Entity<MessagingConnection>()
            .HasIndex(mc => new { mc.Platform, mc.ExternalUserId })
            .IsUnique();

        
        builder.Entity<UserProfile>()
            .Property(u => u.Gender).HasConversion<string>();
        builder.Entity<UserProfile>()
            .Property(u => u.ActivityLevel).HasConversion<string>();
        builder.Entity<UserProfile>()
            .Property(u => u.Goal).HasConversion<string>();

        builder.Entity<MealLog>()
            .Property(ml => ml.MealType).HasConversion<string>();
        builder.Entity<MealLog>()
            .Property(ml => ml.Source).HasConversion<string>();

        builder.Entity<AiRecommendation>()
            .Property(ar => ar.TriggerType).HasConversion<string>();

        builder.Entity<MessagingConnection>()
            .Property(mc => mc.Platform).HasConversion<string>();
    }
}