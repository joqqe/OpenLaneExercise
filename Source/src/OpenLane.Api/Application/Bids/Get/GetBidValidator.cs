﻿using FluentValidation;

namespace OpenLane.Api.Application.Bids.Get;

public class GetBidValidator : AbstractValidator<GetBidRequest>
{
	public GetBidValidator()
	{
		RuleFor(x => x.ObjectId)
			.NotNull().WithMessage("ObjectId is required.")
			.NotEmpty().WithMessage("ObjectId can't be empty.");
	}
}
