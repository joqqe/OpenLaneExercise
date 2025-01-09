namespace OpenLane.Domain.Messages;

public sealed record BidReceivedMessage(Guid IdempotencyKey, Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId)
	: IdempotencyBase(IdempotencyKey);
