namespace AuthServer.API.Dtos;

public class ClientLogInDto
{
	public string ClientId { get; set; } = null!;
	public string ClientSecret { get; set; } = null!;
}
