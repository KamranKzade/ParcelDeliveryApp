using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using SharedLibrary.Models;
using RabbitMQ.Client.Events;
using OrderServer.API.Models;
using SharedLibrary.ResourceFile;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace OrderServer.API.BackgroundServices;

public class DeliveryOrderBackgroundService : BackgroundService
{
	private IModel _channel;
	private readonly IServiceProvider _serviceProvider;
	private readonly RabbitMQClientService _rabbitMqClientService;
	private readonly ILogger<DeliveryOrderBackgroundService> _logger;

	public DeliveryOrderBackgroundService(RabbitMQClientService rabbitMqClientService, ILogger<DeliveryOrderBackgroundService> logger, IServiceProvider dbContext)
	{
		_rabbitMqClientService = rabbitMqClientService;
		_logger = logger;
		_serviceProvider = dbContext;
	}

	public override Task StartAsync(CancellationToken cancellationToken)
	{
		_channel = _rabbitMqClientService.Connect();

		_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

		_logger.LogInformation("DeliveryOrderBackgroundService started.");
		return base.StartAsync(cancellationToken);
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var consumer = new AsyncEventingBasicConsumer(_channel);

		_channel.BasicConsume(RabbitMqClientResource.QueueName, false, consumer);

		consumer.Received += Consumer_Received;

		_logger.LogInformation("Message consumption has started.");
		return Task.CompletedTask;
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping DeliveryOrderBackgroundService...");
		return base.StopAsync(cancellationToken);
	}

	private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
	{
		try
		{
			var orderDelivery = JsonSerializer.Deserialize<OrderDelivery>(Encoding.UTF8.GetString(@event.Body.ToArray()));

			using var scope = _serviceProvider.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			var genericReop = scope.ServiceProvider.GetRequiredService<IGenericRepository<AppDbContext, OrderDelivery>>();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderDelivery.Id);

			order.Status = orderDelivery.Status;
			order.DeliveryDate = orderDelivery.DeliveryDate;
			genericReop.UpdateAsync(order);
			unitOfWork.Commit();

			_logger.LogInformation($"Order updated successfully. OrderId: {order.Id}");
			_channel.BasicAck(@event.DeliveryTag, false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred while processing the order update: {ex.Message}");
			_logger.LogError(ex.Message);
		}

		return Task.CompletedTask;
	}
}
