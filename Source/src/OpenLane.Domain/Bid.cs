namespace OpenLane.Domain;

public class Bid
{
	public int Id { get; set; }
	public Guid ObjectId { get; set; }
	public int OfferId { get; set; }
	public Offer Offer { get; set; } = default!;
	public Guid UserObjectId { get; set; }
	public decimal Price { get; set; }
	public DateTimeOffset ReceivedAt { get; set; }
}
