namespace OpenLane.Domain.Notifications;

public record BidCreatedFailedNotification(Guid BidObjectId, string ErrorMessage);

