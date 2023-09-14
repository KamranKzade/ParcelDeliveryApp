using RabbitMQ.Client;
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
		// Elaqeni yaradiriq
		_connection = _connectionFactory.CreateConnection();

		if (_channel is { IsOpen: true })
		{
			return _channel;
		}

		// Modeli yaradiriq
		_channel = _connection.CreateModel();

		// Exchange-i yaradiriq
		_channel.ExchangeDeclare(RabbitMqClientResource.ExchangeName, type: "direct", durable: true, autoDelete: false);

		// Queue -i yaradiriq
		_channel.QueueDeclare(RabbitMqClientResource.QueueName, durable: true, false, false, null);

		// Queue-ni bind edirik
		_channel.QueueBind(exchange: RabbitMqClientResource.ExchangeName, queue: RabbitMqClientResource.QueueName, routingKey: RabbitMqClientResource.RoutingWaterMark);

		// Log-a informasiyani yaziriq
		_logger.LogInformation("RabbitMQ ile elaqe kuruldu...");

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

