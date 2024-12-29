using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenLane.Api.Domain;

namespace OpenLane.Api.Infrastructure.Configurations;

public class BidEntityConfiguration : IEntityTypeConfiguration<Bid>
{
	public void Configure(EntityTypeBuilder<Bid> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasIndex(x => x.ObjectId)
			.IsUnique();

		builder.Property(x => x.OfferId)
			.IsRequired();

		builder.Property(x => x.Price)
			.IsRequired();

		builder.Property(x => x.User)
			.IsRequired();

		builder.Property(x => x.ReceivedAt)
			.IsRequired();

		builder.HasOne(x => x.Offer)
			.WithMany(x => x.Bids)
			.HasForeignKey(x => x.OfferId);
	}
}
