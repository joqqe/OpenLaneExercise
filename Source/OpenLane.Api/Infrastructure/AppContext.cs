﻿using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Domain;
using OpenLane.Api.Infrastructure.Configurations;

namespace OpenLane.Api.Infrastructure;

public class AppContext : DbContext
{
	public DbSet<Product> Products { get; set; } = default!;
	public DbSet<Offer> Offers { get; set; } = default!;
	public DbSet<Bid> Bids { get; set; } = default!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(BidEntityConfiguration).Assembly);
	}
}
