using OpenLane.Domain;

namespace OpenLane.MessageProcessorTests.TestData;

public class ObjectMother
{
	public ObjectMother()
	{
		OpenOffer = TestDataFactory.CreateOpenOffer();
		ClosedOffer = TestDataFactory.CreateClosedOffer();
		FutureOffer = TestDataFactory.CreateFutureOffer();
		Bid = TestDataFactory.CreateBid(OpenOffer);
		UserObjectId = Guid.NewGuid();
	}

	public Offer OpenOffer { get; init; }
	public Offer ClosedOffer { get; init; }
	public Offer FutureOffer { get; init; }
	public Bid Bid { get; set; }
	public Guid UserObjectId { get; init; }
}