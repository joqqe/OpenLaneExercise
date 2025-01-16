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

		public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout,
			CancellationToken cancellationToken = default)
		{
			var lockValue = Guid.NewGuid().ToString();
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = lockTimeout
			};

			var existingLock = await _cache.GetStringAsync(lockKey, cancellationToken);
			if (existingLock != null)
				return false;

			await _cache.SetStringAsync(lockKey, lockValue, options, cancellationToken);
			return true;
		}

		public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout,
			int retryCount = 0, TimeSpan sleepDuration = default, CancellationToken cancellationToken = default)
		{
			var retries = 0;
			do
			{
				var hasLock = await AcquireLockAsync(lockKey, lockTimeout, cancellationToken);
				if (hasLock)
					return true;

				await Task.Delay(sleepDuration, cancellationToken);

				retries++;
			}
			while (retries != retryCount);

			return false;
		}

		public async Task ReleaseLockAsync(string lockKey, CancellationToken cancellationToken = default)
		{
			await _cache.RemoveAsync(lockKey, cancellationToken);
		}
	}
}
