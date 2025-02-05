namespace OpenLane.Domain.Services;

public interface IIdempotencyService
{
	Task<bool> IsRequestProcessedAsync(string key, string transaction);
	Task MarkRequestAsProcessedAsync(string key, string transaction, TimeSpan expiration);
}
