namespace AuthServer.API.Dtos;

public class UserAppDto
{
	public string? Id { get; set; }
	public string UserName { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? City { get; set; }
}
