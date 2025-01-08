using OpenLane.Domain;

namespace OpenLane.MessageProcessorTests;

public static class TestDataFactory
{
	public static Bid CreateBid(Offer offer, decimal price = 110m)
	{
		return new Bid
		{
			ObjectId = Guid.NewGuid(),
			Offer = offer,
			Price = price,
			ReceivedAt = DateTimeOffset.Now,
			UserObjectId = Guid.NewGuid()
		};
	}

	public static Offer CreateOpenOffer(decimal startingPrice = 100m)
	{
		var productA = new Product
		{
			ObjectId = Guid.NewGuid(),
			Name = "ProductA"
		};
		return new Offer
		{
			ObjectId = Guid.NewGuid(),
			Product = productA,
			StartingPrice = startingPrice,
			OpensAt = DateTimeOffset.Now,
			ClosesAt = DateTimeOffset.Now.AddMonths(1)
		};
	}

	public static Offer CreateClosedOffer(decimal startingPrice = 100m)
	{
		var productB = new Product
		{
			ObjectId = Guid.NewGuid(),
			Name = "ProductB"
		};
		return new Offer
		{
			ObjectId = Guid.NewGuid(),
			Product = productB,
			StartingPrice = startingPrice,
			OpensAt = DateTimeOffset.Now.AddMonths(-2),
			ClosesAt = DateTimeOffset.Now.AddMonths(-1)
		};
	}

	public static Offer CreateFutureOffer(decimal startingPrice = 100m)
	{
		var productC = new Product
		{
			ObjectId = Guid.NewGuid(),
			Name = "ProductC"
		};
		return new Offer
		{
			ObjectId = Guid.NewGuid(),
			Product = productC,
			StartingPrice = startingPrice,
			OpensAt = DateTimeOffset.Now.AddMonths(1),
			ClosesAt = DateTimeOffset.Now.AddMonths(2)
		};
	}
}
