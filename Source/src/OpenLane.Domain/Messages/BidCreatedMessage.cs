namespace OpenLane.Domain.Messages;

public sealed record BidCreatedMessage(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);
