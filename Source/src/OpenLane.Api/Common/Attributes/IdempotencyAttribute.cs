namespace OpenLane.Api.Common.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class IdempotencyAttribute : Attribute
{
	public string TransactionKey { get; }
	public int ExpirationInMinutes { get; }

	public IdempotencyAttribute(string transactionKey, int expirationInMinutes)
	{
		TransactionKey = transactionKey;
		ExpirationInMinutes = expirationInMinutes;
	}
}
