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
	}
}
