namespace OpenLane.Domain.Messages;

public sealed record BidCreatedMessage(Guid IdempontencyKey, Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);
