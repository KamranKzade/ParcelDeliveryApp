using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using SharedLibrary.ResourceFile;

namespace SharedLibrary.Services.RabbitMqCustom;

public class RabbitMQPublisher<TEntity> where TEntity : class
{
	private readonly RabbitMQClientService _rabbitmqClientService;

	public RabbitMQPublisher(RabbitMQClientService rabbitmqClientService)
	{
		_rabbitmqClientService = rabbitmqClientService;
	}


	public void Publish(TEntity entity)
	{
		var channel = _rabbitmqClientService.Connect();

		var bodyString = JsonSerializer.Serialize(entity);
		var bodyByte = Encoding.UTF8.GetBytes(bodyString);

		var property = channel.CreateBasicProperties();
		property.Persistent = true;

		channel.BasicPublish(exchange: RabbitMqClientResource.ExchangeName, routingKey: RabbitMqClientResource.RoutingWaterMark, basicProperties: property, body: bodyByte);
	}
}
