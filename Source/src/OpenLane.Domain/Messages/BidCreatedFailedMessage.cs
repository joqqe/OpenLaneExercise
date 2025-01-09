namespace OpenLane.Domain.Messages;

public record BidCreatedFailedMessage(Guid BidObjectId, string ErrorMessage);
