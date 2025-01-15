using Microsoft.Extensions.Configuration;

namespace OpenLane.ApiTests.Helpers;

public class AccessTokenProvider
{
	private readonly IConfiguration _configuration;

	public AccessTokenProvider(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public string GetToken(string userId)
	{
		var issuer = _configuration.GetValue<string>("Authentication:Issuer")!;
		var audience = _configuration.GetValue<string>("Authentication:Audience")!;
		var secret = _configuration.GetValue<string>("Authentication:Secret")!;
		return JwtTokenHelper.GenerateToken(
			userId,
			"TestUser",
			issuer,
			audience,
			DateTime.Now.AddMinutes(30),
			secret);
	}

	public string GetToken(Guid userId)
	{
		return GetToken(userId.ToString());
	}
}
