using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenLane.Api.Domain;

namespace OpenLane.Api.Infrastructure.Configurations;

public class OfferEntityConfiguration : IEntityTypeConfiguration<Offer>
{
	public void Configure(EntityTypeBuilder<Offer> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasIndex(x => x.ObjectId)
			.IsUnique();

		builder.HasOne(x => x.Product)
			.WithMany(x => x.Offers)
			.HasForeignKey(x => x.ProductId);

		builder.Property(x => x.StartingPrice)
			.IsRequired()
			.HasColumnType("decimal(18, 6))");

		builder.Property(x => x.OpensAt)
			.IsRequired();

		builder.Property(x => x.ClosesAt)
			.IsRequired();
	}
}
