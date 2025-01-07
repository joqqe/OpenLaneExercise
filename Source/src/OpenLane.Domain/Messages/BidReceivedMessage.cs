namespace OpenLane.Domain.Messages;

public record BidReceivedMessage(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);
