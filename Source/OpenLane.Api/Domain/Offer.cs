namespace OpenLane.Api.Domain;

public class Offer
{
	public int Id { get; set; }
	public Guid	ObjectId { get; set; }
	public int ProductId { get; set; }
	public Product Product { get; set; } = default!;
	public ICollection<Bid> Bids { get; set; } = [];
	public decimal StartingPrice { get; set; }
	public DateTimeOffset OpensAt { get; set; }
	public DateTimeOffset ClosesAt { get; set; }
}
