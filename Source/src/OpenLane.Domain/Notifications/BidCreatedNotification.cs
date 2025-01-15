namespace OpenLane.Domain.Notifications;

public sealed record BidCreatedNotification(Guid BidObjectId, Guid OfferObjectId, decimal Price);
