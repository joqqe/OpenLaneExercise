namespace OpenLane.Domain.Messages;

public record BidCreatedMessage(Guid ObjectId, decimal Price, Guid OfferId);
