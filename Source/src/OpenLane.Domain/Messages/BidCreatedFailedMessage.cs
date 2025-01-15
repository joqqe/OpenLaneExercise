namespace OpenLane.Domain.Messages;

public sealed record BidCreatedFailedMessage(Guid IdempotencyKey, Guid BidObjectId, string ErrorMessage, Guid UserObjectId)
	: IdempotencyBase(IdempotencyKey);
