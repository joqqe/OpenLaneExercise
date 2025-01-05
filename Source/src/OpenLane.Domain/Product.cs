namespace OpenLane.Domain;

public class Product
{
	public int Id { get; set; }
	public Guid ObjectId { get; set; }
	public string Name { get; set; } = default!;
	public ICollection<Offer> Offers { get; set; } = [];
}
