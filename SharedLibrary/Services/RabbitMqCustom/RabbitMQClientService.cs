using RabbitMQ.Client;
using SharedLibrary.Helpers;
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

	public IModel Connect(string exchange, string queue, string routingWaterMark)
	{
		var policy = RetryPolicyHelper.GetRetryPolicy();

		IModel? channel = null;

		policy.ExecuteAsync(async () =>
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
				   channel.ExchangeDeclare(exchange, type: "direct", durable: true, autoDelete: false);

				   // Queue -i yaradiriq
				   channel.QueueDeclare(queue, durable: true, false, false, null);

				   // Queue-ni bind edirik
				   channel.QueueBind(exchange: exchange, queue: queue, routingKey: routingWaterMark);

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
