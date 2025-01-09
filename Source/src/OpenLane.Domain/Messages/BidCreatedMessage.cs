namespace OpenLane.Domain.Messages;

public record BidCreatedMessage(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);
