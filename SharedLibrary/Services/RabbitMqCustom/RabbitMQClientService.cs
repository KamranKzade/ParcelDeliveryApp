using RabbitMQ.Client;
using SharedLibrary.Helpers;
using SharedLibrary.ResourceFile;
using Microsoft.Extensions.Logging;

namespace SharedLibrary.Services.RabbitMqCustom;

public class RabbitMQClientService : IDisposable
{
	private IModel _channel;
	private IConnection _connection;
	private readonly ConnectionFactory _connectionFactory;
	private readonly ILogger<RabbitMQClientService> _logger;

	public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
	{
		_connectionFactory = connectionFactory;
		_logger = logger;
	}

	public IModel Connect()
	{
		var policy = RetryPolicyHelper.GetRetryPolicy();

		IModel channel = null;

		policy.Execute(() =>
		{
			try
			{
				// Elaqeni yaradiriq
				_connection = _connectionFactory.CreateConnection();

				if (_channel is { IsOpen: true })
				{
					channel = _channel;
				}
				else
				{
					// Modeli yaradiriq
					channel = _connection.CreateModel();

					// Exchange-i yaradiriq
					channel.ExchangeDeclare(RabbitMqClientResource.ExchangeName, type: "direct", durable: true, autoDelete: false);

					// Queue -i yaradiriq
					channel.QueueDeclare(RabbitMqClientResource.QueueName, durable: true, false, false, null);

					// Queue-ni bind edirik
					channel.QueueBind(exchange: RabbitMqClientResource.ExchangeName, queue: RabbitMqClientResource.QueueName, routingKey: RabbitMqClientResource.RoutingWaterMark);

					// Log-a informasiyani yaziriq
					_logger.LogInformation("RabbitMQ ile elaqe kuruldu...");

					_channel = channel;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error during connection attempt: {ex.Message}");
				throw ex;
			}
		});

		return _channel;
	}
	public void Dispose()
	{
		// Kanali baglayiriq
		_channel?.Close();
		_channel?.Dispose();

		// Connectioni baglayiriq
		_connection?.Close();
		_connection?.Dispose();

		// Loga melumati yaziriq
		_logger.LogInformation("RabbitMQ ile baglanti kopdu...");
	}
}
