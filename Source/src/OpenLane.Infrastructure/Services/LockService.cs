using Microsoft.Extensions.Caching.Distributed;
using OpenLane.Domain.Services;

namespace OpenLane.Infrastructure.Services
{
	public class LockService : ILockService
	{
		private readonly IDistributedCache _cache;

		public LockService(IDistributedCache cache)
		{
			ArgumentNullException.ThrowIfNull(cache);

			_cache = cache;
		}

		public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout)
		{
			var lockValue = Guid.NewGuid().ToString();
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = lockTimeout
			};

			var existingLock = await _cache.GetStringAsync(lockKey);
			if (existingLock == null)
			{
				await _cache.SetStringAsync(lockKey, lockValue, options);
				return true;
			}

			return false;
		}

		public async Task ReleaseLockAsync(string lockKey)
		{
			await _cache.RemoveAsync(lockKey);
		}
	}
}
