namespace OpenLane.Domain.Services;

public interface ILockService
{
	Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout, CancellationToken cancellationToken = default);
	Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout, int retryCount = 0, TimeSpan sleepDuration = default, CancellationToken cancellationToken = default);
	Task ReleaseLockAsync(string lockKey, CancellationToken cancellationToken = default);
}
