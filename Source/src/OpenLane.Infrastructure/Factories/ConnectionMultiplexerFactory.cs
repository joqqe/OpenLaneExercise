using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace OpenLane.Infrastructure.Factories;

public static class ConnectionMultiplexerFactory
{
	private static IConfiguration _configuration = default!;
	private static IConnectionMultiplexer _connectionMultiplexer = default!;

	public static void Initialize(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public static IConnectionMultiplexer Instance()
	{
		if (_connectionMultiplexer is not null)
			return _connectionMultiplexer;

		var connectionStringDistributedCache = _configuration.GetConnectionString("DistributedCache")!;
		_connectionMultiplexer = ConnectionMultiplexer.Connect(connectionStringDistributedCache);

		return _connectionMultiplexer;
	}
}
