using Microsoft.EntityFrameworkCore;
using OpenLane.Common;
using OpenLane.Common.Interfaces;
using OpenLane.Domain;
using OpenLane.Infrastructure;

namespace OpenLane.Api.Application.Bids.Get;

public record GetBidQuery(Guid ObjectId);

public record GetBidResponse(Result<Bid?> Result);

public class GetBidHandler : IHandler<GetBidQuery, GetBidResponse>
{
	private readonly ILogger<GetBidHandler> _logger;
	private readonly AppDbContext _appContext;

	public GetBidHandler(ILogger<GetBidHandler> logger,AppDbContext appContext)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(appContext);

		_logger = logger;
		_appContext = appContext;
	}

	public async Task<GetBidResponse> InvokeAsync(GetBidQuery request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var entity = await _appContext.Bids
			.Include(x => x.Offer)
			.AsNoTracking()
			.SingleOrDefaultAsync(x => x.ObjectId == request.ObjectId, cancellationToken);

		if (entity is null)
		{
			_logger.LogWarning("Bid with ObjectId:{ObjectId} not found", request.ObjectId);
			return new GetBidResponse(Result<Bid?>.Success(default));
		}

		_logger.LogInformation("Successfuly retrieved bid entity: {Entity}", entity);

		return new GetBidResponse(Result<Bid?>.Success(entity));
	}
}
