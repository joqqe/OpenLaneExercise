﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenLane.Domain;

namespace OpenLane.Infrastructure.Configurations;

public class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
{
	public void Configure(EntityTypeBuilder<Product> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasIndex(x => x.ObjectId)
			.IsUnique();

		builder.Property(x => x.Name)
			.IsRequired();
	}
}
