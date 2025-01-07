using Microsoft.EntityFrameworkCore;
using OpenLane.Domain;
using OpenLane.Infrastructure.Configurations;

namespace OpenLane.Infrastructure;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
	{
	}

	public DbSet<Product> Products { get; set; } = default!;
	public DbSet<Offer> Offers { get; set; } = default!;
	public DbSet<Bid> Bids { get; set; } = default!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(BidEntityConfiguration).Assembly);
	}
}
