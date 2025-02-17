namespace OpenLane.Domain.Messages;

public sealed record BidReceivedMessage(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);
