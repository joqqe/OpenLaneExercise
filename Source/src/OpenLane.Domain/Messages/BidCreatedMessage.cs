namespace OpenLane.Domain.Messages;

public sealed record BidCreatedMessage(Guid IdempotencyKey, Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId)
	: IdempotencyBase(IdempotencyKey);
