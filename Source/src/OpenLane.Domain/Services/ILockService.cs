namespace OpenLane.Domain.Services;

public interface ILockService
{
	Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockTimeout);
	Task ReleaseLockAsync(string lockKey);
}
