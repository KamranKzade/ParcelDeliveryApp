namespace AuthServer.API.Models;

public class UserRefleshToken
{
	public string? UserId { get; set; } // User-in Idi
	public string? RefleshToken { get; set; } // Reflesh Token
	public DateTime Expiration { get; set; } // Muddeti
}