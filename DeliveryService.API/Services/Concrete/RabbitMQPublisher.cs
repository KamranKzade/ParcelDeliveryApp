using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using DeliveryServer.API.Models;

namespace DeliveryServer.API.Services.Concrete;

public class RabbitMQPublisher
{
	private readonly RabbitMQClientService _rabbitmqClientService;

	public RabbitMQPublisher(RabbitMQClientService rabbitmqClientService)
	{
		_rabbitmqClientService = rabbitmqClientService;
	}


	public void Publish(OrderDelivery orderDelivetyEvent)
	{
		var channel = _rabbitmqClientService.Connect();

		var bodyString = JsonSerializer.Serialize(orderDelivetyEvent);
		var bodyByte = Encoding.UTF8.GetBytes(bodyString);

		var property = channel.CreateBasicProperties();
		property.Persistent = true;

		channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingWaterMark, basicProperties: property, body: bodyByte);
	}
}
