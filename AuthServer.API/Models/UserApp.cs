using Microsoft.AspNetCore.Identity;

namespace AuthServer.API.Models;

public class UserApp : IdentityUser
{
	public string? Name { get; set; }
	public string? Surname { get; set; }
	public string? Address { get; set; }
	public string? PostalCode { get; set; }
	public DateTime Birhtdate { get; set; }
}
