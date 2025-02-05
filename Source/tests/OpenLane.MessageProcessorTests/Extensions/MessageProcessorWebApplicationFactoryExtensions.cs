using Microsoft.Extensions.DependencyInjection;
using OpenLane.Infrastructure;
using OpenLane.MessageProcessorTests.TestData;

namespace OpenLane.MessageProcessorTests.Extensions;

public static class MessageProcessorWebApplicationFactoryExtensions
{
	public static AppDbContext GetAppDbContext(this MessageProcessorWebApplicationFactory application)
	{
		var serviceProvider = application.Services.GetRequiredService<IServiceProvider>();
		var scope = serviceProvider.CreateScope();
		return scope.ServiceProvider.GetRequiredService<AppDbContext>();
	}

	public static async Task SeedDatabaseAsync(this MessageProcessorWebApplicationFactory application, ObjectMother objectMother)
	{
		var appDbContext = GetAppDbContext(application);

		appDbContext.Offers.Add(objectMother.OpenOffer);
		appDbContext.Offers.Add(objectMother.ClosedOffer);
		appDbContext.Offers.Add(objectMother.FutureOffer);
		appDbContext.Bids.Add(objectMother.Bid);

		await appDbContext.SaveChangesAsync();
	}
}
