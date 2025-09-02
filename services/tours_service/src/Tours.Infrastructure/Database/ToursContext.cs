using Microsoft.EntityFrameworkCore;
using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.Infrastructure.Database
{
  public class ToursContext : DbContext
  {
    public DbSet<Tour> Tours { get; set; }
    public DbSet<TourReview> TourReviews { get; set; }
    public DbSet<Checkpoint> Checkpoints { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<TourPurchaseToken> TourPurchaseTokens { get; set; }
    public DbSet<TourExecution> TourExecutions { get; set; }
    public DbSet<CheckpointProgress> CheckpointProgresses { get; set; }
    public ToursContext(DbContextOptions<ToursContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.HasDefaultSchema("tours");

      // modelBuilder.Entity<Tour>()
      //     .HasMany(t => t.Checkpoints)
      //     .WithMany(c => c.Tours);
      modelBuilder.Entity<TourReview>()
          .HasOne<Tour>()
          .WithMany()
          .HasForeignKey(tr => tr.TourId)
          .OnDelete(DeleteBehavior.Cascade);
          
      modelBuilder.Entity<Tour>()
          .HasMany<Checkpoint>()
          .WithOne()
          .HasForeignKey(c => c.TourId)
          .OnDelete(DeleteBehavior.Cascade);

      // Shopping Cart relationships
      modelBuilder.Entity<ShoppingCart>()
          .HasMany(sc => sc.OrderItems)
          .WithOne()
          .HasForeignKey(oi => oi.ShoppingCartId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<OrderItem>()
          .HasOne<Tour>()
          .WithMany()
          .HasForeignKey(oi => oi.TourId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<ShoppingCart>()
          .Property(sc => sc.UserId)
          .IsRequired();

      // Tour Purchase Token relationships
      modelBuilder.Entity<TourPurchaseToken>()
          .HasOne<Tour>()
          .WithMany()
          .HasForeignKey(tt => tt.TourId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<TourPurchaseToken>()
          .Property(tt => tt.TokenValue)
          .IsRequired();
          
      modelBuilder.Entity<TourPurchaseToken>()
          .Property(tt => tt.UserId)
          .IsRequired();

      // Tour Execution relationships
      modelBuilder.Entity<TourExecution>()
          .HasOne<Tour>()
          .WithMany()
          .HasForeignKey(te => te.TourId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<TourExecution>()
          .HasMany(te => te.CheckpointProgresses)
          .WithOne()
          .HasForeignKey("TourExecutionId")
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<CheckpointProgress>()
          .HasOne<Checkpoint>()
          .WithMany()
          .HasForeignKey(cp => cp.CheckpointId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
}