using PlayPredictorWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using PlayPredictorWebAPI.Models;
using System.Collections.Generic;

namespace g_map_compare_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<ExternalOAuthAccount> ExternalOAuthAccounts => Set<ExternalOAuthAccount>();

        public DbSet<OAuthToken> OAuthTokens => Set<OAuthToken>();

        public DbSet<Calendar> Calendars => Set<Calendar>();

        public DbSet<FaceitAccount> FaceitAccounts => Set<FaceitAccount>();

        public DbSet<Match> Matches => Set<Match>();
        public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
        public DbSet<Analysis> Analyses => Set<Analysis>();
        public DbSet<FeaturedMatchSession> FeaturedMatchSessions => Set<FeaturedMatchSession>();
        public DbSet<SessionPerformance> SessionPerformances => Set<SessionPerformance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum Role als String speichern
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.AuthProvider)
                .HasConversion<string>();

            modelBuilder.Entity<Match>()
                .HasKey(m => new { m.FaceitAccountId, m.MatchId });

            modelBuilder.Entity<Match>()
                .Property(m => m.GameMode)
                .HasConversion<string>();

            modelBuilder.Entity<CalendarEvent>()
                .Property(e => e.Type)
                .HasConversion<string>();

            modelBuilder.Entity<CalendarEvent>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<SessionPerformance>()
                .Property(sp => sp.PerformanceClass)
                .HasConversion<string>();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExternalOAuthAccount>()
                .HasOne(oe => oe.User)
                .WithMany(u => u.ExternalAccounts)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OAuthToken>()
                .HasOne(ot => ot.ExternalOAuthAccount)
                .WithOne(e => e.Token)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Calendar>()
                .Property(c => c.Origin)
                .HasConversion<string>();

            modelBuilder.Entity<Calendar>()
                .HasOne(c => c.User)
                .WithMany(u => u.Calendars)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExternalOAuthAccount>()
                .Property(a => a.Provider)
                .HasConversion<string>();

            modelBuilder.Entity<FaceitAccount>()
                .HasOne(f => f.User)
                .WithOne(u => u.FaceitAccount)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
