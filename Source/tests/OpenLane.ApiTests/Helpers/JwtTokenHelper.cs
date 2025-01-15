using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpenLane.ApiTests.Helpers;

public static class JwtTokenHelper
{
	public static string GenerateToken(string userId, string userName, string issuer, string audience, DateTime expires, string secretKey)
	{
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, userId),
			new Claim(JwtRegisteredClaimNames.UniqueName, userName),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			expires: expires,
			signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
