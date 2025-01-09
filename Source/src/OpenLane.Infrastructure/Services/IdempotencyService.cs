using Microsoft.Extensions.Caching.Distributed;
using OpenLane.Domain.Services;

namespace OpenLane.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
	private readonly IDistributedCache _cache;

	public IdempotencyService(IDistributedCache cache)
	{
		_cache = cache;
	}

	public async Task<bool> IsRequestProcessedAsync(string key)
	{
		var value = await _cache.GetStringAsync(key);
		return value != null;
	}

	public async Task MarkRequestAsProcessedAsync(string key)
	{
		await _cache.SetStringAsync(key, "processed", new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4)
		});
	}
}
