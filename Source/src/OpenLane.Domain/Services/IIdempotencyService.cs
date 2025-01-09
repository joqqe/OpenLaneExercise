namespace OpenLane.Domain.Services;

public interface IIdempotencyService
{
	Task<bool> IsRequestProcessedAsync(string key);
	Task MarkRequestAsProcessedAsync(string key);
}
