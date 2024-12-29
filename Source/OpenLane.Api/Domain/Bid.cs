namespace OpenLane.Api.Domain;

public class Bid
{
	public int Id { get; set; }
	public Guid ObjectId { get; set; }
	public int OfferId { get; set; }
	public Offer Offer { get; set; } = default!;
	public string User { get; set; } = default!;
	public decimal Price { get; set; }
	public DateTimeOffset ReceivedAt { get; set; }
}
