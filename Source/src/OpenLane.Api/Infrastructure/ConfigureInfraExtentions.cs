using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Domain;

namespace OpenLane.Api.Infrastructure;

public static class ConfigureInfraExtentions
{
	public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(options =>
		{
			options
				.UseSqlServer(configuration.GetConnectionString("AppDB"))
				.UseAsyncSeeding(async (context, _, cancellationToken) =>
				{
					// Open offer
					var productA = new Product
					{
						ObjectId = Guid.Parse("47e3f9d5-a32c-4d9a-94cb-79a3fea2368a"),
						Name = "ProductA"
					};
					var openOffer = new Offer
					{
						ObjectId = Guid.Parse("57e3f9d5-a32c-4d9a-94cb-79a3fea2368a"),
						Product = productA,
						StartingPrice = 100m,
						OpensAt = DateTimeOffset.Now,
						ClosesAt = DateTimeOffset.Now.AddMonths(1)
					};
					var newBid = new Bid
					{
						ObjectId = Guid.Parse("67e3f9d5-a32c-4d9a-94cb-79a3fea2368a"),
						Offer = openOffer,
						Price = 110m,
						ReceivedAt = DateTimeOffset.Now,
						UserObjectId = Guid.Parse("67e3f9d5-a32c-4d9a-94cb-79a3fea2368a")
					};
					context.Set<Bid>().Add(newBid);

					await context.SaveChangesAsync(cancellationToken);
				});
		});

		return services;
	}
}
