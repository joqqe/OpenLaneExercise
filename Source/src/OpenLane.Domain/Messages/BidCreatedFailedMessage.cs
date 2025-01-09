namespace OpenLane.Domain.Messages;

public sealed record BidCreatedFailedMessage(Guid BidObjectId, string ErrorMessage);
