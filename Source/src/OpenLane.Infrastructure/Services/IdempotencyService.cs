using Microsoft.Extensions.Caching.Distributed;
using OpenLane.Domain.Services;

namespace OpenLane.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
	private readonly IDistributedCache _cache;

	public IdempotencyService(IDistributedCache cache)
	{
		ArgumentNullException.ThrowIfNull(cache);

		_cache = cache;
	}

	public async Task<bool> IsRequestProcessedAsync(string key, string transaction)
	{
		var value = await _cache.GetStringAsync($"{key}--{transaction}");
		return value != null;
	}

	public async Task MarkRequestAsProcessedAsync(string key, string transaction)
	{
		await _cache.SetStringAsync($"{key}--{transaction}", "processed", new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4)
		});
	}
}
