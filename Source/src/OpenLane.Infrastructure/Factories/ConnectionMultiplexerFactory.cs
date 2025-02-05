using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace OpenLane.Infrastructure.Factories;

public static class ConnectionMultiplexerFactory
{
	public static IConnectionMultiplexer CreateInstance(IConfiguration configuration)
	{
		var connectionStringDistributedCache = configuration.GetConnectionString("DistributedCache")!;
		return ConnectionMultiplexer.Connect(connectionStringDistributedCache);
	}
}
