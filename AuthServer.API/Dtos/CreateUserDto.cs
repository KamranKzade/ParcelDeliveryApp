namespace AuthServer.API.Dtos;

public class CreateUserDto
{
	public string? Name { get; set; }
	public string? FirstName { get; set; }
	public string? Address { get; set; }
	public string? PostalCode { get; set; } = null!;
	public string? Birhtdate { get; set; } = null!;

	public string? UserName { get; set; }
	public string? Email { get; set; }
	public string? Password { get; set; }
}
