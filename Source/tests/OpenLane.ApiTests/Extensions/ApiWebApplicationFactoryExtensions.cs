using Microsoft.Extensions.DependencyInjection;
using OpenLane.ApiTests.Helpers;
using OpenLane.ApiTests.TestData;
using OpenLane.Infrastructure;

namespace OpenLane.ApiTests.Extensions;

public static class ApiWebApplicationFactoryExtensions
{
	public static string GetAccessToken(this ApiWebApplicationFactory application, Guid userObjectId)
	{
		return application.Services
			.GetRequiredService<AccessTokenProvider>()
			.GetToken(userObjectId.ToString());
	}

	public static async Task SeedDatabaseAsync(this ApiWebApplicationFactory application, ObjectMother objectMother)
	{
		var serviceScopeFactory = application.Services.GetRequiredService<IServiceScopeFactory>();
		using var scope = serviceScopeFactory.CreateScope();
		var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		appDbContext.Offers.Add(objectMother.OpenOffer);
		appDbContext.Offers.Add(objectMother.ClosedOffer);
		appDbContext.Offers.Add(objectMother.FutureOffer);
		appDbContext.Bids.Add(objectMother.Bid);

		await appDbContext.SaveChangesAsync();
	}
}
