namespace OpenLane.Domain.Notifications;

public sealed record BidCreatedFailedNotification(Guid BidObjectId, string ErrorMessage);

