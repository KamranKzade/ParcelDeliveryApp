using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using SharedLibrary.Helpers;
using Microsoft.Extensions.Logging;

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


	public void Publish(TEntity entity, string exchange, string queue, string routingWaterMark)
	{
		var policy = RetryPolicyHelper.GetRetryPolicy();

		policy.ExecuteAsync(async () =>
		{
			try
			{
				var channel = _rabbitmqClientService.Connect(exchange, queue, routingWaterMark);

				var bodyString = JsonSerializer.Serialize(entity);
				var bodyByte = Encoding.UTF8.GetBytes(bodyString);

				var property = channel.CreateBasicProperties();
				property.Persistent = true;

				channel.BasicPublish(exchange: exchange, routingKey: routingWaterMark, basicProperties: property, body: bodyByte);

				_logger.LogInformation("The information was successfully published on RabbitMQ");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error during publish: {ex.Message}", ex);
				throw ex!;
			}
		});
	}
}
