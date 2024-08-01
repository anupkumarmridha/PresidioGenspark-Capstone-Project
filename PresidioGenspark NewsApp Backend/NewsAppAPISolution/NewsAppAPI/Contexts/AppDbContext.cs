using Microsoft.EntityFrameworkCore;
using NewsAppAPI.Models;

namespace NewsAppAPI.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            // NewsArticle entity configuration
            modelBuilder.Entity<NewsArticle>()
                .HasKey(na => na.Id);

            // Comment entity configuration
            modelBuilder.Entity<Comment>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Parent)
                .WithMany(p => p.Replies)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.NewsArticle)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique index for comments to ensure a user cannot comment more than once on the same article
            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.UserId, c.ArticleId })
                .IsUnique();

            // Reaction entity configuration
            modelBuilder.Entity<Reaction>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.NewsArticle)
                .WithMany(a => a.Reactions)
                .HasForeignKey(r => r.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique index for reactions to ensure a user cannot react more than once to the same article
            modelBuilder.Entity<Reaction>()
                .HasIndex(r => new { r.UserId, r.ArticleId })
                .IsUnique();
        }
    }
}
