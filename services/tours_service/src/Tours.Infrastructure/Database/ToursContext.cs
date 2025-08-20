using Microsoft.EntityFrameworkCore;
using tours_service.src.Tours.Application.Domain;

namespace tours_service.src.Tours.Infrastructure.Database
{
  public class ToursContext : DbContext
  {
    public DbSet<Tour> Tours { get; set; }
    public DbSet<TourReview> TourReviews { get; set; }
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
    }
  }
}