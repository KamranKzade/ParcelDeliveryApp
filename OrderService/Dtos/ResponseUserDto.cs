namespace OrderServer.API.Dtos;

public class ResponseUserDto
{
	//	[{\"id\":\"d904207b-b597-459f-ae81-deed9cc4ab5a\",\"userName\":\"kami\",\"email\":\"kami@gmail.com\",\"city\":null}]

	public string id { get; set; }
	public string userName { get; set; }
	public string email { get; set; }
	public string? city { get; set; }

}
