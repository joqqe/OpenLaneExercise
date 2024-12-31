using FluentValidation;

namespace OpenLane.Api.Application.Bids.Post
{
	public class PostBidValidator : AbstractValidator<PostBidRequest>
	{
		public PostBidValidator()
		{
			RuleFor(x => x.OfferObjectId)
				.NotNull().WithMessage("OfferObjectId is required.")
				.NotEmpty().WithMessage("OfferObjectId can't be empty.");

			RuleFor(x => x.Price)
				.NotNull().WithMessage("Price is required.")
				.NotEmpty().WithMessage("Price can't be empty.")
				.GreaterThan(0);

			RuleFor(x => x.User)
				.NotNull().WithMessage("User is required.")
				.NotEmpty().WithMessage("User can't be empty.");
		}
	}
}
