using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace OpenLane.MessageProcessorTests.Environment;

public class EnvironmentContainersFixture : IDisposable
{
	public MsSqlContainer MsSqlContainer { get; private set; }
	public RabbitMqContainer RabbitMqContainer { get; private set; }
	public RedisContainer RedisContainer { get; private set; }

	public EnvironmentContainersFixture()
	{
		MsSqlContainer = new MsSqlBuilder().Build();
		RabbitMqContainer = new RabbitMqBuilder().Build();
		RedisContainer = new RedisBuilder().Build();

		Task.WhenAll([
				MsSqlContainer.StartAsync(),
				RabbitMqContainer.StartAsync(),
				RedisContainer.StartAsync()])
			.GetAwaiter().GetResult();
	}

	public void Dispose()
	{
		Task.WhenAll([
			MsSqlContainer.StopAsync(),
			RabbitMqContainer.StopAsync(),
			RedisContainer.StopAsync()])
			.GetAwaiter().GetResult();
		Task.WhenAll([
			MsSqlContainer.DisposeAsync().AsTask(),
			RabbitMqContainer.DisposeAsync().AsTask(),
			RedisContainer.DisposeAsync().AsTask()])
			.GetAwaiter().GetResult();
	}
}
