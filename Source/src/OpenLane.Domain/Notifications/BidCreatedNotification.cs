namespace OpenLane.Domain.Notifications;

public record BidCreatedNotification(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);
