﻿using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Domain;
using OpenLane.Api.Infrastructure;

namespace OpenLane.Api.Application.Bids.Get;

public record GetBidRequest(Guid ObjectId);

public class GetBidHandler : IHandler<GetBidRequest, Result<Bid?>>
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

	public async Task<Result<Bid?>> InvokeAsync(GetBidRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		var entity = await _appContext.Bids.SingleOrDefaultAsync(x => x.ObjectId == request.ObjectId);

		if (entity is null)
		{
			_logger.LogWarning("Bid with ObjectId:{ObjectId} not found", request.ObjectId);
			return Result<Bid?>.Success(default);
		}

		_logger.LogInformation("Successfuly retrieved bid entity: {Entity}", entity);

		return Result<Bid?>.Success(entity);
	}
}
