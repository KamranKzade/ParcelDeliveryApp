using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using SharedLibrary.ResourceFile;
using Microsoft.Extensions.Logging;
using Polly;

namespace SharedLibrary.Services.RabbitMqCustom;

public class RabbitMQPublisher<TEntity> where TEntity : class
{
	private readonly RabbitMQClientService _rabbitmqClientService;
	private readonly ILogger<RabbitMQPublisher<TEntity>> _logger;

	public RabbitMQPublisher(RabbitMQClientService rabbitmqClientService, ILogger<RabbitMQPublisher<TEntity>> logger)
	{
		_rabbitmqClientService = rabbitmqClientService;
		_logger = logger;
	}


	public void Publish(TEntity entity)
	{
		var policy = Policy.Handle<Exception>()
						   .Retry(3, (exception, retryCount) =>
						   {
						   	   _logger.LogWarning($"Retry #{retryCount} after exception: {exception.Message}");
						   });

		policy.Execute(() =>
		{
			try
			{
				var channel = _rabbitmqClientService.Connect();

				var bodyString = JsonSerializer.Serialize(entity);
				var bodyByte = Encoding.UTF8.GetBytes(bodyString);

				var property = channel.CreateBasicProperties();
				property.Persistent = true;

				channel.BasicPublish(exchange: RabbitMqClientResource.ExchangeName, routingKey: RabbitMqClientResource.RoutingWaterMark, basicProperties: property, body: bodyByte);

				_logger.LogInformation("The information was successfully published on RabbitMQ");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error during publish: {ex.Message}", ex);
				throw ex;
			}
		});
	}
}
